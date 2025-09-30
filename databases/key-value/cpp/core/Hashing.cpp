#include <cstdint>
#include <string>
#include "Hashing.h"

uint64_t Hashing::GetHashKey(std::string key)
{
    const uint64_t fnv_offset_basis = 14695981039346656037ULL;
    const uint64_t fnv_prime = 1099511628211ULL;

    uint64_t hash = fnv_offset_basis;

    for (unsigned char c : key)
    {
        hash ^= c;
        hash *= fnv_prime;
    }
    return hash;
};