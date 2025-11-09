"""
Goblin AI - Python Script Example
Chronicles of a Drifter
"""

class GoblinAI:
    """Simple Goblin AI behavior"""
    
    def __init__(self):
        self.state = "patrol"
        self.patrol_timer = 0.0
        self.chase_distance = 5.0
        self.attack_distance = 1.5
    
    def update(self, entity, player_pos, delta_time):
        """
        Update the goblin AI
        
        Args:
            entity: The goblin entity (with position, velocity properties)
            player_pos: Tuple of (x, y) player position
            delta_time: Time elapsed since last update
        
        Returns:
            Dictionary with AI state updates
        """
        # Calculate distance to player
        dx = player_pos[0] - entity.position[0]
        dy = player_pos[1] - entity.position[1]
        distance = (dx * dx + dy * dy) ** 0.5
        
        result = {
            'state': self.state,
            'velocity': (0, 0),
            'should_attack': False
        }
        
        # State machine
        if self.state == "patrol":
            # Patrol behavior
            self.patrol_timer += delta_time
            if self.patrol_timer > 3.0:
                # Change direction every 3 seconds
                import random
                vx = random.uniform(-1, 1)
                vy = random.uniform(-1, 1)
                result['velocity'] = (vx * 0.5, vy * 0.5)
                self.patrol_timer = 0.0
            
            # Check if player is nearby
            if distance < self.chase_distance:
                self.state = "chase"
                result['state'] = "chase"
        
        elif self.state == "chase":
            # Chase the player
            if distance > self.chase_distance * 1.5:
                # Lost the player
                self.state = "patrol"
                result['state'] = "patrol"
            elif distance < self.attack_distance:
                # Close enough to attack
                self.state = "attack"
                result['state'] = "attack"
            else:
                # Move toward player
                if distance > 0:
                    vx = (dx / distance) * 2.0
                    vy = (dy / distance) * 2.0
                    result['velocity'] = (vx, vy)
        
        elif self.state == "attack":
            # Attack the player
            result['should_attack'] = True
            
            # Back to chase after attack
            if distance > self.attack_distance:
                self.state = "chase"
                result['state'] = "chase"
        
        return result

# Factory function for creating AI instances
def create_ai():
    """Create a new goblin AI instance"""
    return GoblinAI()
