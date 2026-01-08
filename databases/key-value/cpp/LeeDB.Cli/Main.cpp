#include <cstdio>
#include <iostream>
#include "Store.h"

const int getMode = 0;
const int putMode = 1;
const int delMode = 2;
int currentMode = -1;
std::string currentKey;
std::string currentValue;


bool HandleInput(std::string command)
{
    if (currentMode == 1)
    {
        if (currentKey.empty())
        {
            currentKey = command;
        }
        else
        {
            currentValue = command;
            return true;
        }
    }

    if (currentMode == 0)
    {
        if (currentKey.empty())
        {
            currentKey = command;
            return true;
        }
    }

    if (currentMode == 2)
    {
        if (currentKey.empty())
        {
            currentKey = command;
            return true;
        }
    }

    if (command == "put")
    {
        currentMode = 1;
    }

    if (command == "get")
    {
        currentMode = 0;
    }

    if (command == "del")
    {
        currentMode = 2;
    }

    return false;
}

void Reset()
{
    currentMode = -1;
    currentKey = "";
    currentValue = "";
    printf("%s", "LeeDB ready >> ");
}

int main(int argc, char* argv[])
{
    printf("%s", "Starting LeeDB ...\n");
    Store* store = new Store();
    printf("%s", "LeeDB ready >> ");

    std::string command;
    while (command != "quit" || command != "exit")
    {
        std::cin >> command;
        bool execute = HandleInput(command);

        if (execute)
        {
            if (currentMode == 0)
            {
                printf("Gettiing %s: %s\n", currentKey.c_str(), store->Get(currentKey).c_str());
            }

            if (currentMode == 1)
            {
                store->Put(currentKey, currentValue);
                printf("Putting %s: %s\n", currentKey.c_str(), currentValue.c_str());
            }

            if (currentMode == 2)
            {
                store->Delete(currentKey);
                printf("Deleting %s\n", currentKey.c_str());
            }

            Reset();
        }
    }

    return 0;
}