#include "PacketFilter.h"      

#ifdef __cplusplus
extern "C" {
#endif

	extern __declspec(dllexport) PacketFilter* CreatePacketFilter();

	extern __declspec(dllexport) void DisposePacketFilter(PacketFilter* a_pObject);

	extern __declspec(dllexport) void StartTheFirewall(PacketFilter* a_pObject);

	extern __declspec(dllexport) void StopTheFirewall(PacketFilter* a_pObject);

	extern __declspec(dllexport) void AddToBlockList(PacketFilter* a_pObject, char* szIpAddrToBlock);

	extern __declspec(dllexport) unsigned int GetBlockListSize(PacketFilter* a_pObject);


#ifdef __cplusplus
}
#endif