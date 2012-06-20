//
//  Timer.cpp
//  DasaniWave
//
//  Created by Damian Stewart on 15/06/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//

#include "DTimer.h"

void DTimer::start( float duration )
{
	if ( isRunning )
		stop();
	remaining = duration;
	ofAddListener( ofEvents().update, this, &DTimer::update );
	isRunning = true;
}

void DTimer::stop( bool triggerEvent )
{
	if ( isRunning ) {
		isRunning = false;
		ofRemoveListener( ofEvents().update, this, &DTimer::update );
		if (triggerEvent) {
			ofNotifyEvent( timerFinishedEv, name, this );
		}
	}
}
	
void DTimer::update( ofEventArgs& args )
{
	remaining -= ofGetLastFrameTime();
	//ofLog() << ofToString(remaining);
	if ( remaining <= 0 )
		stop( true );
}