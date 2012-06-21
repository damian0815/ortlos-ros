#include "testApp.h"

static const float POST_MOVE_PAUSE = 0.5f;
static const float POST_STOP_PAUSE = 0.5f;


static const string HOST = "10.200.1.24";
static const int PORT = 5103;
static const int TRACKING_RECEIVE_PORT = 7001;

static const ofVec2f CORNERS[6] = { ofVec2f(-0.25f, 0), ofVec2f(0.5f, 0), ofVec2f(1.25f, 0), ofVec2f(-0.25f, 1), ofVec2f(0.5f, 1), ofVec2f( 1.25f, 1) };

//--------------------------------------------------------------
void testApp::setup(){

	ofBackground(40, 100, 40);

	// open an outgoing connection to HOST:PORT
	sender.setup(HOST, PORT);

	// open an incoming connection
	receiver.setup( TRACKING_RECEIVE_PORT );
	
	ofSetFrameRate( 60 );
	ofEnableAlphaBlending();
	
	//value = 0.5;
	speed = 0.1f;
	offset = 0.5f;
	doUpdate = true;
	readyToSend = true;
	
	teleskopPositions.resize(6);
	
	endTimer.setName("stop");
	moveTimer.setName("move");
	ofAddListener(endTimer.timerFinishedEv, this, &testApp::timerFired );
	ofAddListener(moveTimer.timerFinishedEv, this, &testApp::timerFired );
}


//--------------------------------------------------------------
void testApp::update(){

	while ( receiver.hasWaitingMessages() )
	{
		ofxOscMessage m;
		receiver.getNextMessage(m);
		if ( m.getAddress() == "/position" )
		{
			int id = m.getArgAsInt32( 0 );
			if ( id <= positions.size() )
			{
				positions.resize(id+1);
				active.resize(id+1);
			}
			bool nowActive = (1==m.getArgAsInt32(1));
			active[id] = nowActive;
			float x = m.getArgAsFloat( 2 );
			float y = m.getArgAsFloat( 3 );
			x = ofMap( x, -120, 120, 0, 1 );
			y = ofMap( y, -120, 120, 0, 1 );
			positions[id] = ofVec2f( x, y );
		}
	}

	updatePositions();

	if ( readyToSend && doUpdate )
	{
		readyToSend = false;
		for ( int i=0; i<6; i++ )
		{
			moveTeleskopTo( i, getPos(i) );
		}
		endTimer.start( POST_MOVE_PAUSE );
	}
}

float testApp::updatePositions()
{
	for ( int i=0; i<6; i++ )
	{
		teleskopPositions[i] = 0.0f;
	}
	// calculate centre of gravity
	ofVec2f centroid;
	int count;
	for ( int i=0; i<positions.size(); i++ )
	{
		if ( active[i] )
		{
			centroid += positions[i];
			count++;
		}
	}
	// if no-one is there we have nothing to do
	if ( count == 0 )
		return;

	centroid /= count;

	// now calculate clumpiness
	float averageDistance = 0;
	count = 0;
	for ( int i=0; i<positions.size(); i++ )
	{
		if ( !active[i] )
			continue;
		for ( int j=i+1; j<positions.size(); j++ )
		{
			if ( !active[j] )
				continue;
			averageDistance += positions[i].distance(positions[j]);
			count++;
		}
	}
	averageDistance /= count;
	float clumpiness = 1.0f-averageDistance;

	// calculate distances to CORNERS
	float distance[6];
	int minDistCorner = -1, maxDistCorner = -1;
	for ( int i=0; i<6; i++ )
	{
		distance[i] = centroid.distance(CORNERS[i]);
		if ( minDistCorner == -1 || distance[i] < distance[minDistCorner] )
		{
			minDistCorner = i;
		}
		if ( maxDistCorner == -1 || distance[i] > distance[maxDistCorner] )
		{
			maxDistCorner = i;
		}
	}

	// normalize distances
	float minDistance = distance[minDistCorner];
	float range = distance[maxDistCorner]-distance[minDistCorner];
	for ( int i=0; i<6 ;i++ )
	{
		distance[i] -= minDistance;
		distance[i] /= range;
	}

	// assign teleskop positions based on closeness
	teleskopPositions[minDistCorner] = 1.0f;
	teleskopPositions[maxDistCorner] = 0.0f;
	for ( int i=0; i<6; i++ )
	{
		if ( i==minDistCorner || i==maxDistCorner  )
			continue;
		float closeness = 1.0f-distance[i];
		closeness *= closeness;
		teleskopPositions[i] = closeness;
	}




}

float testApp::moveTeleskopTo( int which, float position )
{
	ofxOscMessage m;
	m.setAddress("/teleskop/moveTo");
	m.addIntArg(which);
	m.addFloatArg(position);
	sender.sendMessage(m);
}


float testApp::getPos( int whichTeleskop ){
	return 0.5f+0.5f*sinf(ofGetElapsedTimef()*speed + offset*whichTeleskop);
	return teleskopPositions[whichTeleskop];
}



//--------------------------------------------------------------
void testApp::draw(){
	// display instructions
	string buf;
	buf = "sending osc messages to " + string(HOST) + ":" + ofToString(PORT);
	ofSetColor(ofColor::white, 255);
	ofDrawBitmapString(buf, 10, 20);
	ofDrawBitmapString("press M to toggle updating (currently "+ofToString(doUpdate)+")", 10, 50 );
	//ofDrawBitmapString("press up/down to change value ("+ofToString(value)+")", 10, 65 );
	ofDrawBitmapString("press up/down to change speed     ("+ofToString(speed)+")", 10, 65 );
	ofDrawBitmapString("press left/right to change offset ("+ofToString(offset)+")", 10, 80 );
	
	for ( int i=0; i<6; i++ )
	{
		float baseHeight = 200;
		float x = CORNERS[i].x * ofGetWidth();
		float y = CORNERS[i].y * ofGetHeight();
		ofNoFill();
		ofSetColor( ofColor::white, 255 );
		ofRect( x, y, 40, baseHeight );
		ofFill();
		ofSetColor( ofColor::white, 64 );
		ofRect( x, y+(1.0f-getPos(i))*baseHeight, 40, getPos(i)*baseHeight );
	}
}

void testApp::timerFired( string &timerName )
{
	if ( timerName == "stop" )
	{
		//ofLog() << "stop timer fired";
		for ( int i=0; i<6; i++ )
		{
			ofxOscMessage m;
			m.setAddress( "/teleskop/stop" );
			m.addIntArg( i );
			sender.sendMessage(m);
		}
		moveTimer.start( POST_STOP_PAUSE );
	}
	else if ( timerName == "move" )
	{
		//ofLog() << "move timer fired";
		readyToSend = true;
	}
}

//--------------------------------------------------------------
void testApp::keyPressed(int key){
	if ( key == 'm' || key == 'M' ){
		doUpdate= !doUpdate;
	}
	/*
	else if ( key == OF_KEY_UP )
		value = min(1.0f,value+0.05f);
	else if ( key == OF_KEY_DOWN )
		value = max(0.0f,value-0.05f);
	 */
	else if ( key == OF_KEY_UP )
		speed += 0.05f;
	else if ( key == OF_KEY_DOWN )
		speed -= 0.05f;
	else if ( key == OF_KEY_LEFT )
		offset += 0.05f;
	else if ( key == OF_KEY_RIGHT )
		offset -= 0.05f;
}

//--------------------------------------------------------------
void testApp::keyReleased(int key){

}

//--------------------------------------------------------------
void testApp::mouseMoved(int x, int y){
}

//--------------------------------------------------------------
void testApp::mouseDragged(int x, int y, int button){
}

//--------------------------------------------------------------
void testApp::mousePressed(int x, int y, int button){
}

//--------------------------------------------------------------
void testApp::mouseReleased(int x, int y, int button){
}

//--------------------------------------------------------------
void testApp::windowResized(int w, int h){

}

//--------------------------------------------------------------
void testApp::gotMessage(ofMessage msg){

}

//--------------------------------------------------------------
void testApp::dragEvent(ofDragInfo dragInfo){

}

