// grace_pf.h : main header file for the grace_pf DLL
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'pch.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CgracepfApp
// See grace_pf.cpp for the implementation of this class
//

class CgracepfApp : public CWinApp
{
public:
	CgracepfApp();

// Overrides
public:
	virtual BOOL InitInstance();

	DECLARE_MESSAGE_MAP()
};
