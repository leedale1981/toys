import pygame
from pygame.math import Vector2

from body import Body

pygame.init()

WIDTH, HEIGHT = 800, 600
GRAVITY = Vector2(0, 500)
FLOOR_Y = HEIGHT - 50
BOUNCINESS = -0.9

screen = pygame.display.set_mode((WIDTH, HEIGHT))
pygame.display.set_caption("Newtonian Physics Sim")

clock = pygame.time.Clock()
running = True
ball = Body((400, 100), mass=1.0)


def HandleInputEvents():
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            running = False


def GetDeltaTime():
    return clock.tick(60) / 1000.0


def ApplyGravity():
    ball.apply_force(GRAVITY * ball.mass)


def DetectFloor():
    if ball.pos.y + ball.radius > FLOOR_Y:
        ball.pos.y = FLOOR_Y - ball.radius
        ball.vel.y *= BOUNCINESS


def UpdateAndRenderBodies():
    ball.update(deltaTime)
    DetectFloor()
    screen.fill((20, 20, 30))
    ball.draw(screen)
    RenderFloor()
    pygame.display.flip()


def RenderFloor():
    pygame.draw.line(screen, (150, 150, 150),
                     (0, FLOOR_Y), (WIDTH, FLOOR_Y), 2)


while running:
    deltaTime = GetDeltaTime()
    HandleInputEvents()
    ApplyGravity()
    UpdateAndRenderBodies()

pygame.quit()
