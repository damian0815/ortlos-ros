//
//  Timer.h
//  DasaniWave
//
//  Created by Damian Stewart on 15/06/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//

#pragma once

#include "ofMain.h"

class DTimer
{
public:
	DTimer():isRunning(false) {};
	
	void setName( string _name ) { name =_name; }
	void start( float duration );
	void stop( bool triggerEvent=false );
	
	ofEvent<string> timerFinishedEv;
	
private:
	
	void update(ofEventArgs &args );
	
	string name;
	float remaining;
	bool isRunning;
	
};

