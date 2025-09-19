#include <cstdint>
#include <string>
#include "Store.h"

void Store::Put(uint64_t key, std::string &value)
{
    if (key >= mBackingStore->length())
    {
        ResizeBackingStore();
    }
    mBackingStore[key] = value;
}

void Store::ResizeBackingStore()
{
}