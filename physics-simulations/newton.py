import pygame
from pygame.math import Vector2

from body import Body

pygame.init()

WIDTH, HEIGHT = 800, 600
GRAVITY = Vector2(0, 500)
screen = pygame.display.set_mode((WIDTH, HEIGHT))
pygame.display.set_caption("Newtonian Physics Sim")

clock = pygame.time.Clock()
running = True

while running:
    deltaTime = clock.tick(60) / 1000.0

    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            running = False

    screen.fill((20, 20, 30))

    ball = Body((400, 100), mass=1.0)
    ball.apply_force(GRAVITY * ball.mass)
    ball.update(deltaTime)
    ball.draw(screen)

    pygame.display.flip()

pygame.quit()
