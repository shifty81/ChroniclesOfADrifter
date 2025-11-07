# Lua Scripting API Documentation

## Overview

This document provides examples of how Lua can be used to script enemy behaviors, weapon effects, and game logic in Chronicles of a Drifter.

## Lua Integration Architecture

The game uses **NLua** or **MoonSharp** (C# Lua interpreter) to execute Lua scripts from the .NET 9 game logic layer.

### Script Loading System
```csharp
public class LuaScriptManager
{
    private Script luaEngine;
    private Dictionary<string, DynValue> loadedScripts;
    
    public LuaScriptManager()
    {
        luaEngine = new Script();
        
        // Register C# API for Lua scripts
        RegisterGameAPI();
        
        // Enable hot-reloading
        EnableFileWatcher();
    }
    
    private void RegisterGameAPI()
    {
        // Register types that Lua can access
        UserData.RegisterType<Vector2>();
        UserData.RegisterType<Entity>();
        UserData.RegisterType<Component>();
        
        // Register global functions
        luaEngine.Globals["GetEntity"] = (Func<int, Entity>)GetEntity;
        luaEngine.Globals["SpawnProjectile"] = (Action<string, Vector2, Vector2>)SpawnProjectile;
        luaEngine.Globals["PlaySound"] = (Action<string>)PlaySound;
        luaEngine.Globals["DealDamage"] = (Action<Entity, float, string>)DealDamage;
    }
}
```

## Enemy AI Scripting

### Example 1: Basic Patrol Enemy

**File**: `scripts/lua/enemies/goblin_patrol.lua`

```lua
-- Goblin Patrol AI
-- This enemy patrols between waypoints and attacks when player is near

-- State machine states
local State = {
    PATROL = 1,
    CHASE = 2,
    ATTACK = 3,
    RETREAT = 4
}

-- Enemy configuration
local config = {
    detectionRange = 8.0,
    attackRange = 1.5,
    patrolSpeed = 2.0,
    chaseSpeed = 4.0,
    retreatHealthPercent = 0.3,
    attackCooldown = 1.5,
    patrolWaypoints = {}
}

-- Enemy state
local state = {
    currentState = State.PATROL,
    currentWaypoint = 1,
    lastAttackTime = 0,
    target = nil
}

-- Called once when enemy spawns
function OnSpawn(entity, waypoints)
    config.patrolWaypoints = waypoints
    state.currentState = State.PATROL
    state.currentWaypoint = 1
end

-- Called every frame
function OnUpdate(entity, deltaTime)
    local player = GetNearestPlayer(entity)
    local distanceToPlayer = Vector2.Distance(entity.position, player.position)
    
    -- State transitions
    if state.currentState == State.PATROL then
        if distanceToPlayer < config.detectionRange then
            state.currentState = State.CHASE
            state.target = player
            PlaySound("goblin_alert")
        else
            UpdatePatrol(entity, deltaTime)
        end
        
    elseif state.currentState == State.CHASE then
        local healthPercent = entity.health / entity.maxHealth
        
        if healthPercent < config.retreatHealthPercent then
            state.currentState = State.RETREAT
        elseif distanceToPlayer < config.attackRange then
            state.currentState = State.ATTACK
        elseif distanceToPlayer > config.detectionRange * 1.5 then
            state.currentState = State.PATROL
        else
            UpdateChase(entity, player, deltaTime)
        end
        
    elseif state.currentState == State.ATTACK then
        if distanceToPlayer > config.attackRange then
            state.currentState = State.CHASE
        else
            UpdateAttack(entity, player, deltaTime)
        end
        
    elseif state.currentState == State.RETREAT then
        UpdateRetreat(entity, player, deltaTime)
    end
end

-- Patrol behavior
function UpdatePatrol(entity, deltaTime)
    local targetWaypoint = config.patrolWaypoints[state.currentWaypoint]
    local direction = Vector2.Normalize(targetWaypoint - entity.position)
    
    entity.velocity = direction * config.patrolSpeed
    
    -- Check if reached waypoint
    if Vector2.Distance(entity.position, targetWaypoint) < 0.5 then
        state.currentWaypoint = (state.currentWaypoint % #config.patrolWaypoints) + 1
    end
    
    SetAnimation(entity, "walk")
end

-- Chase behavior
function UpdateChase(entity, player, deltaTime)
    local direction = Vector2.Normalize(player.position - entity.position)
    entity.velocity = direction * config.chaseSpeed
    
    SetAnimation(entity, "run")
end

-- Attack behavior
function UpdateAttack(entity, player, deltaTime)
    entity.velocity = Vector2.Zero()
    
    local currentTime = GetGameTime()
    if currentTime - state.lastAttackTime > config.attackCooldown then
        PerformAttack(entity, player)
        state.lastAttackTime = currentTime
    end
    
    SetAnimation(entity, "attack")
end

-- Retreat behavior
function UpdateRetreat(entity, player, deltaTime)
    local direction = Vector2.Normalize(entity.position - player.position)
    entity.velocity = direction * config.patrolSpeed * 1.5
    
    SetAnimation(entity, "run")
    
    -- Return to patrol after getting far enough
    if Vector2.Distance(entity.position, player.position) > config.detectionRange * 2 then
        state.currentState = State.PATROL
        entity.health = entity.maxHealth -- Heal up
    end
end

function PerformAttack(entity, player)
    PlaySound("goblin_attack")
    DealDamage(player, 10.0, "physical")
    SpawnEffect("slash_effect", player.position)
end

-- Called when enemy takes damage
function OnDamage(entity, damage, damageType)
    if damage > 20 then
        state.currentState = State.RETREAT
    end
end

-- Called when enemy dies
function OnDeath(entity)
    SpawnLoot(entity.position, "goblin_loot_table")
    PlaySound("goblin_death")
    SpawnEffect("death_effect", entity.position)
end
```

### Example 2: Flying Enemy (Ranged)

**File**: `scripts/lua/enemies/bat_ranged.lua`

```lua
-- Flying Bat AI
-- Flies in circular patterns and shoots projectiles

local config = {
    attackRange = 10.0,
    projectileSpeed = 8.0,
    circleRadius = 5.0,
    circleSpeed = 2.0,
    shootCooldown = 2.0,
    burstCount = 3,
    burstDelay = 0.3
}

local state = {
    angle = 0,
    centerPoint = nil,
    lastShootTime = 0,
    burstCounter = 0
}

function OnSpawn(entity)
    state.centerPoint = entity.position
    state.angle = math.random() * math.pi * 2
end

function OnUpdate(entity, deltaTime)
    local player = GetNearestPlayer(entity)
    local distanceToPlayer = Vector2.Distance(entity.position, player.position)
    
    -- Update circle position
    state.centerPoint = Vector2.Lerp(state.centerPoint, player.position, deltaTime * 0.5)
    
    -- Fly in circle around target point
    state.angle = state.angle + config.circleSpeed * deltaTime
    local offset = Vector2.new(
        math.cos(state.angle) * config.circleRadius,
        math.sin(state.angle) * config.circleRadius
    )
    
    local targetPosition = state.centerPoint + offset
    local direction = Vector2.Normalize(targetPosition - entity.position)
    entity.velocity = direction * 5.0
    
    -- Shoot at player
    if distanceToPlayer < config.attackRange then
        local currentTime = GetGameTime()
        
        if state.burstCounter < config.burstCount then
            if currentTime - state.lastShootTime > config.burstDelay then
                ShootAtPlayer(entity, player)
                state.lastShootTime = currentTime
                state.burstCounter = state.burstCounter + 1
            end
        else
            if currentTime - state.lastShootTime > config.shootCooldown then
                state.burstCounter = 0
            end
        end
    end
    
    SetAnimation(entity, "fly")
end

function ShootAtPlayer(entity, player)
    local direction = Vector2.Normalize(player.position - entity.position)
    SpawnProjectile("bat_projectile", entity.position, direction * config.projectileSpeed)
    PlaySound("bat_shoot")
end

function OnDeath(entity)
    SpawnLoot(entity.position, "bat_loot_table")
end
```

### Example 3: Boss AI (State Machine)

**File**: `scripts/lua/enemies/boss_skeleton_king.lua`

```lua
-- Skeleton King Boss AI
-- Multi-phase boss with different attack patterns

local Phase = {
    PHASE_1 = 1,  -- Melee attacks
    PHASE_2 = 2,  -- Summons minions
    PHASE_3 = 3   -- Ranged and AOE attacks
}

local Attack = {
    MELEE_COMBO = 1,
    GROUND_SLAM = 2,
    SUMMON_SKELETONS = 3,
    BONE_BARRAGE = 4,
    DEATH_CIRCLE = 5
}

local config = {
    phase2Threshold = 0.66,
    phase3Threshold = 0.33,
    attackCooldown = 3.0,
    meleeDamage = 25.0,
    slamDamage = 40.0,
    slamRadius = 5.0
}

local state = {
    currentPhase = Phase.PHASE_1,
    currentAttack = nil,
    lastAttackTime = 0,
    attackInProgress = false,
    comboStep = 0
}

function OnSpawn(entity)
    state.currentPhase = Phase.PHASE_1
    PlayMusic("boss_theme")
    ShowBossHealthBar(entity)
end

function OnUpdate(entity, deltaTime)
    local healthPercent = entity.health / entity.maxHealth
    
    -- Phase transitions
    if healthPercent < config.phase3Threshold and state.currentPhase ~= Phase.PHASE_3 then
        TransitionToPhase(entity, Phase.PHASE_3)
    elseif healthPercent < config.phase2Threshold and state.currentPhase == Phase.PHASE_1 then
        TransitionToPhase(entity, Phase.PHASE_2)
    end
    
    -- Attack logic
    if not state.attackInProgress then
        local currentTime = GetGameTime()
        if currentTime - state.lastAttackTime > config.attackCooldown then
            ChooseAndExecuteAttack(entity)
            state.lastAttackTime = currentTime
        end
    end
    
    UpdateCurrentAttack(entity, deltaTime)
end

function TransitionToPhase(entity, newPhase)
    state.currentPhase = newPhase
    state.attackInProgress = false
    
    if newPhase == Phase.PHASE_2 then
        PlaySound("boss_roar")
        ShowMessage("The Skeleton King summons his army!")
        SummonSkeletons(entity, 4)
    elseif newPhase == Phase.PHASE_3 then
        PlaySound("boss_rage")
        ShowMessage("The Skeleton King enters a rage!")
        entity.attackSpeed = entity.attackSpeed * 1.5
    end
end

function ChooseAndExecuteAttack(entity)
    local attacks = GetAvailableAttacks(state.currentPhase)
    local chosenAttack = attacks[math.random(#attacks)]
    
    state.currentAttack = chosenAttack
    state.attackInProgress = true
    
    if chosenAttack == Attack.MELEE_COMBO then
        StartMeleeCombo(entity)
    elseif chosenAttack == Attack.GROUND_SLAM then
        StartGroundSlam(entity)
    elseif chosenAttack == Attack.SUMMON_SKELETONS then
        SummonSkeletons(entity, 3)
    elseif chosenAttack == Attack.BONE_BARRAGE then
        StartBoneBarrage(entity)
    elseif chosenAttack == Attack.DEATH_CIRCLE then
        StartDeathCircle(entity)
    end
end

function GetAvailableAttacks(phase)
    if phase == Phase.PHASE_1 then
        return {Attack.MELEE_COMBO, Attack.GROUND_SLAM}
    elseif phase == Phase.PHASE_2 then
        return {Attack.MELEE_COMBO, Attack.GROUND_SLAM, Attack.SUMMON_SKELETONS}
    else
        return {Attack.GROUND_SLAM, Attack.BONE_BARRAGE, Attack.DEATH_CIRCLE}
    end
end

function StartMeleeCombo(entity)
    state.comboStep = 1
    SetAnimation(entity, "attack_1")
    ScheduleCallback(0.5, function()
        DealDamageInMeleeRange(entity, config.meleeDamage)
        state.comboStep = 2
    end)
end

function StartGroundSlam(entity)
    SetAnimation(entity, "ground_slam_charge")
    ScheduleCallback(1.0, function()
        PlaySound("ground_slam")
        CameraShake(0.5, 10.0)
        DealDamageInRadius(entity.position, config.slamRadius, config.slamDamage)
        SpawnEffect("ground_slam_effect", entity.position)
        state.attackInProgress = false
    end)
end

function SummonSkeletons(entity, count)
    SetAnimation(entity, "summon")
    for i = 1, count do
        local offset = Vector2.new(
            math.cos(i / count * math.pi * 2) * 3,
            math.sin(i / count * math.pi * 2) * 3
        )
        SpawnEnemy("skeleton_minion", entity.position + offset)
    end
    state.attackInProgress = false
end

function OnDeath(entity)
    PlaySound("boss_death")
    ShowMessage("The Skeleton King has been defeated!")
    SpawnLoot(entity.position, "boss_loot_epic")
    TriggerCutscene("boss_death_sequence")
end
```

## Weapon Behavior Scripting

### Example: Fire Sword with DoT Effect

**File**: `scripts/lua/weapons/fire_sword.lua`

```lua
-- Fire Sword Weapon Script
-- Applies burning DoT effect on hit

local weaponConfig = {
    baseDamage = 15.0,
    burnDamage = 3.0,
    burnDuration = 5.0,
    burnTickRate = 1.0,
    critChance = 0.15,
    critMultiplier = 2.0,
    swingSpeed = 1.2,
    knockbackForce = 5.0
}

function OnEquip(entity, weapon)
    weapon.attackSpeed = weaponConfig.swingSpeed
    PlaySound("equip_sword")
end

function OnAttack(entity, weapon, hitEntities)
    for _, target in ipairs(hitEntities) do
        local damage = weaponConfig.baseDamage
        
        -- Critical hit chance
        if math.random() < weaponConfig.critChance then
            damage = damage * weaponConfig.critMultiplier
            SpawnEffect("critical_hit", target.position)
            PlaySound("critical_hit")
        end
        
        -- Deal base damage
        DealDamage(target, damage, "fire")
        
        -- Apply burning effect
        ApplyStatusEffect(target, "burning", {
            damagePerTick = weaponConfig.burnDamage,
            tickRate = weaponConfig.burnTickRate,
            duration = weaponConfig.burnDuration
        })
        
        -- Knockback
        local knockbackDir = Vector2.Normalize(target.position - entity.position)
        ApplyKnockback(target, knockbackDir * weaponConfig.knockbackForce)
        
        -- Visual effects
        SpawnEffect("fire_slash", target.position)
        PlaySound("fire_hit")
    end
end

function OnUnequip(entity, weapon)
    -- Cleanup
end
```

### Example: Lightning Staff

**File**: `scripts/lua/weapons/lightning_staff.lua`

```lua
-- Lightning Staff Weapon Script
-- Chains lightning between enemies

local weaponConfig = {
    baseDamage = 20.0,
    chainCount = 3,
    chainRange = 6.0,
    manaCost = 15,
    castTime = 0.5,
    chainDamageMultiplier = 0.7
}

function OnAttack(entity, weapon, targetPosition)
    -- Check mana
    if entity.mana < weaponConfig.manaCost then
        PlaySound("no_mana")
        return
    end
    
    entity.mana = entity.mana - weaponConfig.manaCost
    
    -- Cast animation delay
    SetAnimation(entity, "cast")
    ScheduleCallback(weaponConfig.castTime, function()
        CastLightning(entity, targetPosition)
    end)
end

function CastLightning(entity, targetPosition)
    local hitTargets = {}
    local currentPosition = entity.position
    local currentDamage = weaponConfig.baseDamage
    
    -- Find initial target
    local firstTarget = GetNearestEnemyInRange(currentPosition, weaponConfig.chainRange)
    if firstTarget == nil then
        return
    end
    
    -- Chain lightning
    local chainTarget = firstTarget
    for i = 1, weaponConfig.chainCount do
        if chainTarget == nil then break end
        
        -- Deal damage
        DealDamage(chainTarget, currentDamage, "lightning")
        table.insert(hitTargets, chainTarget)
        
        -- Visual effect
        SpawnLightningBolt(currentPosition, chainTarget.position)
        SpawnEffect("lightning_impact", chainTarget.position)
        
        -- Stun chance
        if math.random() < 0.3 then
            ApplyStatusEffect(chainTarget, "stunned", {duration = 1.0})
        end
        
        -- Find next target
        currentPosition = chainTarget.position
        currentDamage = currentDamage * weaponConfig.chainDamageMultiplier
        chainTarget = GetNearestEnemyInRange(currentPosition, weaponConfig.chainRange, hitTargets)
    end
    
    PlaySound("lightning_strike")
    CameraShake(0.2, 5.0)
end
```

## Quest Scripting

### Example: Fetch Quest

**File**: `scripts/lua/quests/gather_herbs.lua`

```lua
-- Gather Herbs Quest

local questData = {
    id = "gather_herbs_001",
    title = "Gather Healing Herbs",
    description = "Collect 10 healing herbs from the forest for the village healer.",
    requiredItems = {
        {item = "healing_herb", count = 10}
    },
    rewards = {
        gold = 100,
        experience = 50,
        items = {
            {item = "health_potion", count = 3}
        }
    }
}

function OnQuestStart(player)
    ShowMessage("Quest Started: " .. questData.title)
    AddQuestToJournal(player, questData)
end

function OnItemCollected(player, itemId, count)
    if itemId == "healing_herb" then
        UpdateQuestProgress(player, questData.id)
        
        local currentCount = GetItemCount(player, "healing_herb")
        if currentCount >= 10 then
            ShowMessage("Quest objective complete! Return to the healer.")
            SetQuestObjectiveComplete(player, questData.id, 1)
        end
    end
end

function OnQuestComplete(player)
    -- Remove items
    RemoveItem(player, "healing_herb", 10)
    
    -- Give rewards
    AddGold(player, questData.rewards.gold)
    AddExperience(player, questData.rewards.experience)
    
    for _, reward in ipairs(questData.rewards.items) do
        AddItem(player, reward.item, reward.count)
    end
    
    ShowMessage("Quest Complete! Received rewards.")
    PlaySound("quest_complete")
end
```

## Performance Considerations

### Best Practices
1. **Avoid Per-Frame Allocations**: Reuse tables and objects
2. **Batch Operations**: Group multiple similar operations
3. **Use Local Variables**: Faster than global lookups
4. **Cache Function Results**: Don't recalculate every frame
5. **Limit Script Complexity**: Keep scripts focused and simple

### Example Optimization
```lua
-- Bad: Creates new table every frame
function OnUpdate(entity, deltaTime)
    local data = {x = 1, y = 2, z = 3}
    -- use data
end

-- Good: Reuse table
local data = {x = 0, y = 0, z = 0}
function OnUpdate(entity, deltaTime)
    data.x = 1
    data.y = 2
    data.z = 3
    -- use data
end
```

## Debugging Lua Scripts

### Debug Functions
```lua
-- Print to console
DebugLog("Enemy health: " .. entity.health)

-- Draw debug visualization
DebugDrawLine(start, end, Color.Red, 1.0)
DebugDrawCircle(position, radius, Color.Green)

-- Breakpoint (pauses game)
DebugBreak()
```

## Conclusion

Lua scripting provides powerful flexibility for Chronicles of a Drifter, allowing rapid iteration on enemy behaviors, weapon effects, and quest logic without recompiling the core engine. The examples above demonstrate the breadth of possibilities available through the Lua API.
