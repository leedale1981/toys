import pygame
from pygame.math import Vector2


class Body:
    def __init__(self, pos, mass=1.0):
        self.pos = Vector2(pos)
        self.vel = Vector2(0, 0)
        self.force = Vector2(0, 0)
        self.mass = mass
        self.radius = 10

    def apply_force(self, force):
        self.force += force

    def update(self, deltaTime):
        acceleration = self.force / self.mass

        # Euler integration
        self.vel = acceleration * deltaTime
        self.pos = self.vel * deltaTime

        self.force.update(0, 0)

    def draw(self, screen):
        pygame.draw.circle(
            screen, (200, 200, 255),
            self.pos, self.radius
        )
