-- Goblin AI with chase behavior for Chronicles of a Drifter
-- Chases player when in detection range

local State = {
    IDLE = 1,
    PATROL = 2,
    CHASE = 3,
    ATTACK = 4
}

local goblin = {
    state = State.IDLE,
    detectionRange = 300.0,
    attackRange = 75.0,
    speed = 80.0,
    spawnTime = 0,
    patrolTimer = 0,
    patrolDirection = 1
}

function goblin.OnSpawn(entityId, position)
    print("Goblin " .. entityId .. " spawned at (" .. position.X .. ", " .. position.Y .. ")")
    goblin.state = State.PATROL
    goblin.spawnTime = 0
    goblin.patrolDirection = (math.random() > 0.5) and 1 or -1
end

function goblin.OnUpdate(entityId, deltaTime, position, velocity, playerPosition, playerDistance)
    goblin.spawnTime = goblin.spawnTime + deltaTime
    goblin.patrolTimer = goblin.patrolTimer + deltaTime
    
    -- Check if we should chase player
    if playerDistance and playerDistance < goblin.detectionRange then
        goblin.state = State.CHASE
        
        if playerPosition and position then
            -- Calculate direction to player
            local dx = playerPosition.X - position.X
            local dy = playerPosition.Y - position.Y
            local distance = math.sqrt(dx * dx + dy * dy)
            
            if distance > 0 then
                -- Normalize and apply speed
                velocity.VX = (dx / distance) * goblin.speed
                velocity.VY = (dy / distance) * goblin.speed
            end
        end
    else
        -- Simple patrol behavior
        goblin.state = State.PATROL
        
        -- Change direction every 2 seconds
        if goblin.patrolTimer > 2.0 then
            goblin.patrolDirection = -goblin.patrolDirection
            goblin.patrolTimer = 0
        end
        
        velocity.VX = goblin.patrolDirection * goblin.speed * 0.5
        velocity.VY = 0
    end
end

function goblin.OnDeath(entityId)
    print("Goblin " .. entityId .. " defeated!")
end

return goblin
