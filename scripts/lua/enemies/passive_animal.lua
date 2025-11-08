-- Passive animal AI for Chronicles of a Drifter
-- Wanders around, flees from player when close

local State = {
    WANDER = 1,
    FLEE = 2,
    IDLE = 3
}

local animal = {
    state = State.WANDER,
    fleeRange = 150.0,
    speed = 100.0,
    wanderTimer = 0,
    wanderDirection = { x = 0, y = 0 },
    idleTimer = 0
}

function animal.OnSpawn(entityId, position)
    print("Passive animal " .. entityId .. " spawned at (" .. position.X .. ", " .. position.Y .. ")")
    animal.state = State.WANDER
    animal.wanderTimer = 0
    animal.idleTimer = 0
    -- Random initial wander direction
    local angle = math.random() * math.pi * 2
    animal.wanderDirection.x = math.cos(angle)
    animal.wanderDirection.y = math.sin(angle)
end

function animal.OnUpdate(entityId, deltaTime, position, velocity, playerPosition, playerDistance)
    animal.wanderTimer = animal.wanderTimer + deltaTime
    animal.idleTimer = animal.idleTimer + deltaTime
    
    -- Check if player is too close - flee!
    if playerDistance and playerDistance < animal.fleeRange and playerPosition and position then
        animal.state = State.FLEE
        
        -- Calculate direction away from player
        local dx = position.X - playerPosition.X
        local dy = position.Y - playerPosition.Y
        local distance = math.sqrt(dx * dx + dy * dy)
        
        if distance > 0 then
            -- Normalize and apply speed
            velocity.VX = (dx / distance) * animal.speed * 1.5  -- Flee faster!
            velocity.VY = (dy / distance) * animal.speed * 1.5
        end
        
        animal.idleTimer = 0
    else
        -- Not fleeing - wander or idle
        
        -- Alternate between wandering and being idle
        if animal.state == State.WANDER and animal.wanderTimer > 3.0 then
            -- Switch to idle
            animal.state = State.IDLE
            velocity.VX = 0
            velocity.VY = 0
            animal.wanderTimer = 0
            animal.idleTimer = 0
        elseif animal.state == State.IDLE and animal.idleTimer > 2.0 then
            -- Switch to wander with new random direction
            animal.state = State.WANDER
            local angle = math.random() * math.pi * 2
            animal.wanderDirection.x = math.cos(angle)
            animal.wanderDirection.y = math.sin(angle)
            animal.wanderTimer = 0
            animal.idleTimer = 0
        end
        
        -- Apply wander movement
        if animal.state == State.WANDER then
            velocity.VX = animal.wanderDirection.x * animal.speed * 0.5
            velocity.VY = animal.wanderDirection.y * animal.speed * 0.5
        end
    end
end

function animal.OnDeath(entityId)
    print("Passive animal " .. entityId .. " died")
end

return animal
