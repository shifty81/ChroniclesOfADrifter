-- Example Fire Sword Weapon Script
-- Planning phase example

local fireSword = {
    name = "Fire Sword",
    baseDamage = 15.0,
    attackSpeed = 1.2,
    specialEffect = "burning"
}

function OnEquip(player)
    print("Fire Sword equipped!")
end

function OnAttack(attacker, target)
    print("Fire Sword attack! Damage: " .. fireSword.baseDamage)
    -- Apply burning effect
end

return fireSword
