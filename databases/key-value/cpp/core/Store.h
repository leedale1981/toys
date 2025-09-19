#include <cstdint>
#include <string>

class Store
{
private:
    std::string mBackingStore[3];
    void ResizeBackingStore();

public:
    void Put(uint64_t key, std::string &value);
};