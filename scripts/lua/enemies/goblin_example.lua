-- Example Goblin Enemy AI for Chronicles of a Drifter
-- Demonstrates Lua scripting integration with the ECS

local State = {
    IDLE = 1,
    PATROL = 2,
    CHASE = 3,
    ATTACK = 4
}

local goblin = {
    state = State.IDLE,
    detectionRange = 10.0,
    attackRange = 2.0,
    speed = 3.0,
    spawnTime = 0
}

function goblin.OnSpawn(entityId, position)
    print("Goblin spawned! Entity ID: " .. entityId)
    if position then
        print("  Position: " .. position.X .. ", " .. position.Y)
    end
    goblin.state = State.PATROL
    goblin.spawnTime = 0
end

function goblin.OnUpdate(entityId, deltaTime, position)
    goblin.spawnTime = goblin.spawnTime + deltaTime
    
    -- Simple patrol behavior - just log occasionally
    if math.floor(goblin.spawnTime * 2) % 5 == 0 and goblin.spawnTime - deltaTime < math.floor(goblin.spawnTime * 2) / 2 then
        print("Goblin " .. entityId .. " is patrolling...")
    end
end

function goblin.OnDeath(entityId)
    print("Goblin " .. entityId .. " defeated!")
    -- Spawn loot would go here
end

return goblin
