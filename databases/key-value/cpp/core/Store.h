#include <cstdint>
#include <string>
#include <vector>
#include "Hashing.h"

class Store
{
private:
    static const int mInitialSize = 64;
    int mArrayCount = 0;
    std::vector<std::string> mBackingStore;
    Hashing *mHashing;

    void ResizeBackingStore();

public:
    Store();
    void Put(std::string key, std::string value);
    std::string Get(std::string key);
};