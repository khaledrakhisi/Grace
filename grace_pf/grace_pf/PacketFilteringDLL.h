// PacketFilteringDLL.h : main header file for the PacketFilteringDLL DLL
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'pch.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CPacketFilteringDLLApp
// See PacketFilteringDLL.cpp for the implementation of this class
//

class CPacketFilteringDLLApp : public CWinApp
{
public:
	CPacketFilteringDLLApp();

// Overrides
public:
	virtual BOOL InitInstance();

	DECLARE_MESSAGE_MAP()
};
