#include "testApp.h"

static const int PORT = 7000;

static const int NUM_RECEIVERS = 7;
static const string RECEIVER[NUM_RECEIVERS] = { 
	"91.143.110.164",
	"91.143.110.166",
	"91.143.110.170",
	"91.143.110.172",
	"91.143.110.168",
	"91.143.110.169",
	"91.143.110.171",
};

//--------------------------------------------------------------
void testApp::setup(){
	// listen on the given port
	cout << "listening for osc messages on port " << PORT << "\n";
	receiver.setup(PORT);

	for ( int i=0; i<NUM_RECEIVERS; i++ )
	{
		// memory leak
		senders.push_back( ofPtr(new ofxOscSender()) );
		senders.back()->setup( RECEIVER[i], 7001 );
	}

	ofBackground(30, 30, 130);

}

//--------------------------------------------------------------
void testApp::update(){

	// check for waiting messages
	while(receiver.hasWaitingMessages()){
		// get the next message
		ofxOscMessage m;
		receiver.getNextMessage(&m);

		for ( int i=0; i<senders.size(); i++ )
		{
			senders[i]->sendMessage(m);
		}

		// create a string representing the message
		string msg_string;
		msg_string = ofToString( (int)ofGetElapsedTimef() );
		msg_string += " "+m.getAddress();
		msg_string += ": ";
		for(int i = 0; i < m.getNumArgs(); i++){
			// get the argument type
			msg_string += m.getArgTypeName(i);
			msg_string += ":";
			// display the argument - make sure we get the right type
			if(m.getArgType(i) == OFXOSC_TYPE_INT32){
				msg_string += ofToString(m.getArgAsInt32(i));
			}
			else if(m.getArgType(i) == OFXOSC_TYPE_FLOAT){
				msg_string += ofToString(m.getArgAsFloat(i));
			}
			else if(m.getArgType(i) == OFXOSC_TYPE_STRING){
				msg_string += m.getArgAsString(i);
			}
			else{
				msg_string += "unknown";
			}
		}
		lastMessageString = msg_string;

	}
}


//--------------------------------------------------------------
void testApp::draw(){

	string buf;
	buf = "listening for osc messages on port" + ofToString(PORT);
	ofDrawBitmapString(buf, 10, 20);

	ofDrawBitmapString(lastMessageString, 10, 55 );



}

//--------------------------------------------------------------
void testApp::keyPressed(int key){

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
