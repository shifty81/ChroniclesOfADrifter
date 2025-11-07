-- Example Goblin Enemy AI for Chronicles of a Drifter
-- This is a planning phase example

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
    speed = 3.0
}

function OnSpawn(entity)
    print("Goblin spawned at position: " .. entity.position.x .. ", " .. entity.position.y)
    goblin.state = State.PATROL
end

function OnUpdate(entity, deltaTime)
    -- AI logic would be implemented here
    -- This is a placeholder for the planning phase
end

function OnDeath(entity)
    print("Goblin defeated!")
    -- Spawn loot
end

return goblin
