#include <cstdint>
#include <string>

class Store
{
public:
    Store();
    void Put(std::string key, std::string value);
    std::string Get(std::string key);
    void Delete(std::string key);
};