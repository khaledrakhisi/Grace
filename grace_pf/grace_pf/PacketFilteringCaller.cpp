#include "PacketFilteringCaller.h"


PacketFilter* CreatePacketFilter()
{
	/*PacketFilter pktFilter = PacketFilter();
	pktFilter.AddToBlockList("172.27.20.83");
	pktFilter.AddToBlockList("172.27.10.14");*/
	//return pktFilter;
	return new PacketFilter();
}

void DisposePacketFilter(PacketFilter* a_pObject)
{
	if (a_pObject != NULL)
	{
		delete a_pObject;
		a_pObject = NULL;
	}
}

void StartTheFirewall(PacketFilter* a_pObject)
{
	if (a_pObject != NULL)
	{
		//a_pObject->AddToBlockList("172.27.10.14");
		a_pObject->StartFirewall();
	}
}

void StopTheFirewall(PacketFilter* a_pObject)
{
	if (a_pObject != NULL)
	{		
		a_pObject->StopFirewall();
	}
}

void AddToBlockList(PacketFilter* a_pObject, char* szIpAddr)
{
	if (a_pObject != NULL && szIpAddr != NULL)
	{
		a_pObject->AddToBlockList(szIpAddr);
	}
}

unsigned int GetBlockListSize(PacketFilter* a_pObject)
{
	if (a_pObject != NULL)
	{
		return a_pObject->GetBlockListSize();
	}
	return NULL;
}