#include "..\core\Store.h"

void Test()
{
    Store *store = new Store();
    store->Put("item1", "value1");
    store->Put("item2", "value2");
    store->Put("item3", "value3");

    std::string value = store->Get("item1");
    printf("%s", value.c_str());
}