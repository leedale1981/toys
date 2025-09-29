#include "..\core\Store.h"

void main()
{
    Store *store = new Store();
    store->Put("item1", "value1");

    std::string value = store->Get("item1");
    printf("%s", value);
}