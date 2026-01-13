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
ball = Body((400, 100), mass=1.0)

while running:
    deltaTime = clock.tick(60) / 1000.0

    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            running = False

    ball.apply_force(GRAVITY * ball.mass)
    ball.update(deltaTime)
    screen.fill((20, 20, 30))
    ball.draw(screen)

    pygame.display.set_caption(
        f"dt={deltaTime:.4f}  pos=({ball.pos.x:.1f},{ball.pos.y:.1f})  vel=({ball.vel.x:.1f},{ball.vel.y:.1f})")

    pygame.display.flip()

pygame.quit()
