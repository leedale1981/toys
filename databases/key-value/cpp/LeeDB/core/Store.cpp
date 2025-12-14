#include <cstdint>
#include <string>
#include <fstream>
#include "Store.h"

using namespace std;

Store::Store()
{
    mHashing = new Hashing();
    LoadData();
}

void Store::Put(string key, string value)
{
    uint64_t hashKey = mHashing->GetHashKey(key);
    int arrayIndex = hashKey % mBucketSize;
    mBackingStore[arrayIndex] = value;
    Flush();
    mArrayCount++;
}

string Store::Get(string key)
{
    uint64_t hashKey = mHashing->GetHashKey(key);
    int arrayIndex = hashKey % mBucketSize;
    return mBackingStore[arrayIndex];
}

void Store::Flush()
{
    FILE* file;
    fopen_s(&file, mFileName.c_str(), "wb");

    if (file != NULL)
    {
        fwrite(mBackingStore, sizeof(mBackingStore), mArrayCount, file);
        fclose(file);
    }
}

void Store::LoadData()
{
    ifstream inStream(mFileName, ios::binary);

    if (!inStream)
    {
        return;
    }

    uint64_t fileBucketSize = 0;
    inStream.read(reinterpret_cast<char *>(&fileBucketSize), sizeof(fileBucketSize));

    if (!inStream)
    {
        throw std::runtime_error("Failed to read bucket size");
    }

    if (fileBucketSize != mBucketSize)
    {
        throw std::runtime_error("Bucket size in file does not match compiled size");
    }

    for (uint64_t index = 0; index < mBucketSize; ++index)
    {
        uint64_t len = 0;
        inStream.read(reinterpret_cast<char *>(&len), sizeof(len));

        if (!inStream)
        {
            throw std::runtime_error("Unexpected end of file while reading string length");
        }

        std::string value;

        if (len)
        {
            value.resize(static_cast<size_t>(len));
            inStream.read(value.data(), static_cast<std::streamsize>(len));

            if (!inStream)
            {
                throw std::runtime_error("Unexpected end of file while reading string data");
            }
        }

        mBackingStore[index] = std::move(value);
    }

    if (!inStream.good() && !inStream.eof())
    {
        throw std::runtime_error("Read error");
    }
}