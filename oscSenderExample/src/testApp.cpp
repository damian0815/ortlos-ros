#include "testApp.h"

static const float POST_MOVE_PAUSE = 0.25f;
static const float POST_STOP_PAUSE = 0.1f;


static const string HOST = "91.143.110.164";
static const int PORT = 5103;

//--------------------------------------------------------------
void testApp::setup(){

	ofBackground(40, 100, 40);

	// open an outgoing connection to HOST:PORT
	sender.setup(HOST, PORT);
	
	ofSetFrameRate( 60 );
	ofEnableAlphaBlending();
	
	//value = 0.5;
	position = 0.1f;
	offset = 0.5f;
	doUpdate = true;
	readyToSend = true;
	
	
	endTimer.setName("stop");
	moveTimer.setName("move");
	ofAddListener(endTimer.timerFinishedEv, this, &testApp::timerFired );
	ofAddListener(moveTimer.timerFinishedEv, this, &testApp::timerFired );
}


//--------------------------------------------------------------
void testApp::update(){

	if ( readyToSend && doUpdate )
	{
		readyToSend = false;
		for ( int i=0; i<6; i++ )
		{
			ofxOscMessage m;
			m.setAddress("/teleskop/moveTo");
			m.addIntArg(i);
			m.addFloatArg(getPos(i));
			sender.sendMessage(m);
		}
		endTimer.start( POST_MOVE_PAUSE );
	}
}

float testApp::getPos( int whichTeleskop ){
	return 0.0;
	// return fabs(sin(double(whichTeleskop)));
	// return min(1.0f,max(0.0f,position));
	//return 0.5f+0.5f*sinf(ofGetElapsedTimef()*position + offset*whichTeleskop);
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
	
	ofDrawBitmapString("press up/down to change position    ("+ofToString(position)+")", 10, 65 );
	//ofDrawBitmapString("press left/right to change offset ("+ofToString(offset)+")", 10, 80 );
	
	for ( int i=0; i<6; i++ )
	{
		float baseHeight = 200;
		ofNoFill();
		ofSetColor( ofColor::white, 255 );
		ofRect( 30+i*50, 100, 40, baseHeight );
		ofFill();
		ofSetColor( ofColor::white, 64 );
		ofRect( 30+i*50, 100+(1.0f-getPos(i))*baseHeight, 40, getPos(i)*baseHeight );
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
		position += 0.05f;
	else if ( key == OF_KEY_DOWN )
		position -= 0.05f;
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

