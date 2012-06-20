using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using LogicData.VirtualHandset;

namespace VirtualHandsetTest
{
    public partial class frmMain : Form
    {
        /// <summary>
        /// create VirtualHandset object
        /// </summary>
        VirtualHandset Handset = new VirtualHandset();

        /// <summary>
        /// flag indicating that we should save a memory-position instead of moving to it
        /// </summary>
        Boolean DoSaveMemoPosition = false;


        public frmMain()
        {
            InitializeComponent();
        }
        
        private void frmMain_Load(object sender, EventArgs e)
        {
            // add click-event handler for the memory-buttons
            this.btnMemo1.Click += new EventHandler(this.btnMemoX_Click);
            this.btnMemo2.Click += new EventHandler(this.btnMemoX_Click);
            this.btnMemo3.Click += new EventHandler(this.btnMemoX_Click);
            this.btnMemo4.Click += new EventHandler(this.btnMemoX_Click);

            // add mouse-down handler for the memory-buttons
            this.btnMemo1.MouseDown += new MouseEventHandler(this.btnMemoX_MouseDown);
            this.btnMemo2.MouseDown += new MouseEventHandler(this.btnMemoX_MouseDown);
            this.btnMemo3.MouseDown += new MouseEventHandler(this.btnMemoX_MouseDown);
            this.btnMemo4.MouseDown += new MouseEventHandler(this.btnMemoX_MouseDown);

            // add mouse-up handler for the memory-buttons
            this.btnMemo1.MouseUp += new MouseEventHandler(this.btnMemoX_MouseUp);
            this.btnMemo2.MouseUp += new MouseEventHandler(this.btnMemoX_MouseUp);
            this.btnMemo3.MouseUp += new MouseEventHandler(this.btnMemoX_MouseUp);
            this.btnMemo4.MouseUp += new MouseEventHandler(this.btnMemoX_MouseUp);

            // add event-handler for updating the display
            Handset.OnUpdateDisplay += new VirtualHandset.OnUpdateDisplayDelegate(OnUpdateDisplay);
            // add event-handler for synchronizing the PC after driving
            Handset.OnSynchronizeAfterDriving += new VirtualHandset.OnSynchronizeAfterDrivingDelegate(OnSynchronizeAfterDriving);

            // add the list of available COM-ports
            cbxComPort.Items.AddRange(Handset.GetAvailableComPorts());
            // check if there are no items in the list (i.e. no COM-ports were found)
            if (cbxComPort.Items.Count == 0)
            {
                MessageBox.Show("Could not find any COM-ports on your system. Please install drivers.", 
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // FOR DEBUGING: select your default COM-port below so you don't need 
            //               to select the port every time.
            //cbxComPort.SelectedItem = "COM2";
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // check if the virtual handset is currently connected
            if (Handset.IsConnected())
            {
                // disconnect the virtual handset when you close the app!
                Handset.Disconnect();
            }
        }

        /// <summary>
        /// event-handler that is called when the handset-display should be updated
        /// </summary>
        /// <param name="HandsetDisplay">current handset-display</param>
        private void OnUpdateDisplay(VirtualHandset.HandsetDisplay HandsetDisplay)
        {
            // INFO: you can update your handset-display here.

            // check if no invoke is required (we are running on the same thread as the event-caller)
            if (!txtHandsetDisplay.InvokeRequired)
            {
                // check if we should NOT save a a memory-position --> we update the display instead
                if (!DoSaveMemoPosition)
                {
                    // update the display
                    txtHandsetDisplay.Text = HandsetDisplay.DisplayText;
                }
            }
            // invoke is required (we are running on a different thread as the event-caller)
            else
            {
                // invoke the display-update delegate
                // ATTENTION! Be sure to use BeginInvoke(), because Invoke() would create dead-locks as it waits for completetion!
                txtHandsetDisplay.BeginInvoke(new VirtualHandset.OnUpdateDisplayDelegate(OnUpdateDisplay), HandsetDisplay);
            }
        }

        /// <summary>
        /// event-handler that is called when the PC should be synchronized after driving
        /// </summary>
        private void OnSynchronizeAfterDriving()
        {
            // INFO: you can upload the current control-unit settings here
            //       to get the driving statistics, positions, etc.

            VirtualHandset.ControlUnitSettings Settings = new VirtualHandset.ControlUnitSettings();

            // get all settings from the control-unit and check for error
            if (Handset.GetAllSettings(ref Settings) < 0)
            {
                MessageBox.Show("Could not get settings!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // inform the virtual handset that we have finished synchronizing our data
            // NOTE: if you do not upload data, you do not need to call this function.
            Handset.SynchronizeAfterDrivingFinished();
        }

        #region "Events of Virtual Handset UI"

        private void btnUp1_MouseDown(object sender, MouseEventArgs e)
        {
            // start moving upwards with motor-group 1
            Handset.BeginMove(VirtualHandset.MovingDirection.Upwards, VirtualHandset.MotorGroup.MotorGroup1);
        }

        private void btnUp1_MouseUp(object sender, MouseEventArgs e)
        {
            // end moving
            Handset.EndMove();
        }

        private void btnDown1_MouseDown(object sender, MouseEventArgs e)
        {
            // start moving downwards with motor-group 1
            Handset.BeginMove(VirtualHandset.MovingDirection.Downwards, VirtualHandset.MotorGroup.MotorGroup1);
        }

        private void btnDown1_MouseUp(object sender, MouseEventArgs e)
        {
            // end moving
            Handset.EndMove();
        }

        private void btnUp2_MouseDown(object sender, MouseEventArgs e)
        {
            // start moving upwards with motor-group 2
            Handset.BeginMove(VirtualHandset.MovingDirection.Upwards, VirtualHandset.MotorGroup.MotorGroup2);
        }

        private void btnUp2_MouseUp(object sender, MouseEventArgs e)
        {
            // end moving
            Handset.EndMove();
        }

        private void btnDown2_MouseDown(object sender, MouseEventArgs e)
        {
            // start moving downwards with motor-group 2
            Handset.BeginMove(VirtualHandset.MovingDirection.Downwards, VirtualHandset.MotorGroup.MotorGroup2);
        }

        private void btnDown2_MouseUp(object sender, MouseEventArgs e)
        {
            // end moving
            Handset.EndMove();
        }

        private void btnSaveMemo_Click(object sender, EventArgs e)
        {
            // NOTE: Reset drive (re-align) of the the drives needs to be done by pressing [DOWN] 
            //       for long time until the control-unit finished the reset drive.
            //       Pressing the S-Button in not emulated by the virtual handset!

            // check if we have not clicked the S-Button yet
            if (!DoSaveMemoPosition)
            {
                // set flag indicating we should save a memory-position istead of moving to it
                // TODO: you should use a timeout to reset this flag after a few seconds (for the user-application)
                DoSaveMemoPosition = true;
                // display "S -" on the handset
                txtHandsetDisplay.Text = "S -";
            }
            // we already clicked the S-Button
            else
            {
                // clear flag indicating we should save a memory-position istead of moving to it
                DoSaveMemoPosition = false;
                // clear the handset-display
                txtHandsetDisplay.Text = "";
            }
        }

        private void btnMemoX_Click(object sender, EventArgs e)
        {
            string ButtonName = ((Button)sender).Name;
            // calculate the memory-position-number from the buttons end-number
            VirtualHandset.MemoNumber MemoNumber = (VirtualHandset.MemoNumber)Convert.ToInt32(ButtonName.Substring(ButtonName.Length - 1, 1)) - 1;

            // check if we should save a a memory-position istead of moving to it
            if (DoSaveMemoPosition)
            {
                // display "S 1" to "S 6" on the handset
                txtHandsetDisplay.Text = "S " + ((int)MemoNumber + 1);
                // save the memory-position x
                Handset.SaveMemoryPosition(MemoNumber);
                // NOTE: we don't clear the DoSaveMemoPosition flag here, 
                // because we get the MouseDown-event after the Click-event!
            }
        }

        private void btnMemoX_MouseDown(object sender, MouseEventArgs e)
        {
            // check if we should NOT save a a memory-position istead of moving to it
            if (!DoSaveMemoPosition)
            {
                string ButtonName = ((Button)sender).Name;
                // calculate the memory-position-number from the buttons end-number
                VirtualHandset.MemoNumber MemoNumber = (VirtualHandset.MemoNumber)Convert.ToInt32(ButtonName.Substring(ButtonName.Length - 1, 1)) - 1;

                // begin moving to the memory-position x
                Handset.BeginMove(MemoNumber);
            }
        }

        private void btnMemoX_MouseUp(object sender, MouseEventArgs e)
        {
            // check if we should NOT save a a memory-position istead of moving to it
            if (!DoSaveMemoPosition)
            {
                // end moving
                Handset.EndMove();
            }
            // clear flag indicating we should save a memory-position istead of moving to it
            DoSaveMemoPosition = false;
        }

        #endregion

        #region "Events of other buttons"

        private void cbxComPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            // check if an item is selected
            if ((string)cbxComPort.SelectedItem != null)
            {
                this.Enabled = false;

                // check if the virtual handset is currently connected
                if (Handset.IsConnected())
                {
                    // disconnect the virtual handset
                    Handset.Disconnect();
                }

                // connect the virtual handset to the control-unit and check for error
                if (Handset.Connect((string)cbxComPort.SelectedItem) < 0)
                {
                    MessageBox.Show("Could not connect the virtual-handset to \"" + (string)cbxComPort.SelectedItem + "\".", 
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                this.Enabled = true;
            }
        }

        private void btnFindControlUnit_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            cbxComPort.Enabled = false;

            // check if the virtual handset is currently connected
            if (Handset.IsConnected())
            {
                // disconnect the virtual handset
                Handset.Disconnect();
            }

            // get the port of the first found control-unit
            // NOTE: this function can take long time to finish,
            //       so prefer to do this on first launch of the app and then
            //       save the COM-port in your application settings.
            string PortNameOfFoundControlUnit = Handset.GetPortOfFirstFoundControlUnit();

            // check if we have not found a control-unit
            if (PortNameOfFoundControlUnit == "")
            {
                MessageBox.Show("No control-unit found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                try {
                    // do not select any items
                    cbxComPort.SelectedItem = null;
                    // try to select the found COM-port in the list
                    cbxComPort.SelectedItem = PortNameOfFoundControlUnit;
                } catch {
                    // do not select any items on error
                    cbxComPort.SelectedItem = null;
                }
            }

            cbxComPort.Enabled = true;
            this.Enabled = true;
        }

        private void btnGetAllSettings_Click(object sender, EventArgs e)
        {
            this.Enabled = false;

            VirtualHandset.ControlUnitSettings Settings = new VirtualHandset.ControlUnitSettings();

            // get all settings from the control-unit and check for error
            if (Handset.GetAllSettings(ref Settings) < 0)
            {
                MessageBox.Show("Could not get settings!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // NOTE: use the "watch" feature to examine the information in the Settings-variable.
            //       plenty of information can be read from there.
            // TO BE ADDED: process the settings and display information.

            this.Enabled = true;
        }

        private void btnGetDisplayedError_Click(object sender, EventArgs e)
        {
            this.Enabled = false;

            string DisplayedError = "";

            // get the displayed error
            if (Handset.GetDisplayedError(ref DisplayedError) < 0)
            {
                MessageBox.Show("Could not get displayed error!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                // check if no error is displayed
                if (DisplayedError == "")
                {
                    MessageBox.Show("no error is currently displayed.", "Displayed Error", MessageBoxButtons.OK);
                }
                // we have an error to display
                else
                {
                    // show the displayed error in a message-box
                    MessageBox.Show("Displayed Error: " + DisplayedError, "Displayed Error", MessageBoxButtons.OK);
                }
            }

            this.Enabled = true;
        }

        private void btnWriteOEMInfo_Click(object sender, EventArgs e)
        {
            this.Enabled = false;

            // create dummy OEM-info for testing...
            byte[] OEMInfo = { 0x01, 0x02, 0x03, 0x04 };

            // write the OEM-information (4 bytes) and check for error
            if (Handset.WriteOEMInformation(OEMInfo) < 0)
            {
                MessageBox.Show("Could not write OEM-Information!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            this.Enabled = true;
        }

        private void btnWriteMemoPositions_Click(object sender, EventArgs e)
        {
            this.Enabled = false;

            // create new array that holds the array of memo-positions for each motor-group
            int[][] MemoPositions = new int[VirtualHandset.MAX_MF_MOTOR_GROUPS][];
            // create new arrays for the memory-positions
            for (int MotGroup = 0; MotGroup < VirtualHandset.MAX_MF_MOTOR_GROUPS; MotGroup++)
            {
                // create the array for the memory-positions
                MemoPositions[MotGroup] = new int[VirtualHandset.MAX_MEMO_POSITIONS];
            }

            // build some dummy memory-positions for motor-group 1
            MemoPositions[0][0] = 100;
            MemoPositions[0][1] = 200;
            MemoPositions[0][2] = 300;
            MemoPositions[0][3] = 400;
            MemoPositions[0][4] = 500;
            MemoPositions[0][5] = 600;

            // build some dummy memory-positions for motor-group 2
            MemoPositions[1][0] = 110;
            MemoPositions[1][1] = 210;
            MemoPositions[1][2] = 310;
            MemoPositions[1][3] = 410;
            MemoPositions[1][4] = 510;
            MemoPositions[1][5] = 610;


            // simple flag to check if we had an error writing the memo-positions
            bool ErrorWriting = false;

            // let's write all memory-positions...
            if (Handset.WriteMemoryPosition(MemoPositions) < 0)
            {
                ErrorWriting = true;
            }

            // let's write only a single memory-position...
            if (Handset.WriteMemoryPosition(VirtualHandset.MotorGroup.MotorGroup1, VirtualHandset.MemoNumber.Memo2, 30) < 0)
            {
                ErrorWriting = true;
            }

            // check if we had at least one error
            if (ErrorWriting)
            {
                MessageBox.Show("Could not write memory-positions!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            this.Enabled = true;
        }

        private void btnWriteContainerAndShelfStop_Click(object sender, EventArgs e)
        {
            this.Enabled = false;

            //create an array of the container- and shelf-stop settings for every motor-group
            VirtualHandset.ContainerShelfStopSettings[] Settings = 
                new VirtualHandset.ContainerShelfStopSettings[VirtualHandset.MAX_MF_MOTOR_GROUPS];

            // build some dummy container- and shelf-stop settings for motor-group 1
            Settings[0].ContainerStopActive = true;
            Settings[0].ContainerStopPosition = 100;
            Settings[0].ShelfStopActive = true;
            Settings[0].ShelfStopPosition = 500;

            // build some dummy container- and shelf-stop settings for motor-group 2
            Settings[1].ContainerStopActive = true;
            Settings[1].ContainerStopPosition = 110;
            Settings[1].ShelfStopActive = true;
            Settings[1].ShelfStopPosition = 510;

            // simple flag to check if we had an error writing the memo-positions
            bool ErrorWriting = false;

            // let's write all container- and shelf-stop settings...
            if (Handset.WriteContainerAndShelfStopSetting(Settings) < 0)
            {
                ErrorWriting = true;
            }

            // let's write only one container- and shelf-stop settings...
            if (Handset.WriteContainerAndShelfStopSetting(VirtualHandset.MotorGroup.MotorGroup1, Settings[0]) < 0)
            {
                ErrorWriting = true;
            }

            // let's write only a container-stop setting...
            if (Handset.WriteContainerStopSetting(VirtualHandset.MotorGroup.MotorGroup1, 50, true) < 0)
            {
                ErrorWriting = true;
            }

            // let's write only a shelf-stop setting...
            if (Handset.WriteShelfStopSetting(VirtualHandset.MotorGroup.MotorGroup1, 550, true) < 0)
            {
                ErrorWriting = true;
            }

            // check if we had at least one error
            if (ErrorWriting)
            {
                MessageBox.Show("Could not write container- and shelf-stop settings!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            this.Enabled = true;
        }

        private void btnAutoMoveToPosition_Click(object sender, EventArgs e)
        {
            this.Enabled = false;

            // let's automatically move to a specified position with motor-group1...
            if (Handset.AutoMoveToPosition(200, VirtualHandset.MotorGroup.MotorGroup1) < 0)
            {
                MessageBox.Show("Could not automatically move to position!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            this.Enabled = true;
        }

        private void btnAutoMoveToMemoPosition_Click(object sender, EventArgs e)
        {
            this.Enabled = false;

            // let's automatically move to memo-position 2
            if (Handset.AutoMoveToMemoryPosition(VirtualHandset.MemoNumber.Memo2) < 0)
            {
                MessageBox.Show("Could not automatically move to memo-position!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            this.Enabled = true;
        }

        private void btnSetContainerOrShelfStop_Click(object sender, EventArgs e)
        {
            ((Button)sender).Enabled = false;

            // let's activate the container- or shelf-stop for our current position
            if (Handset.SetContainerOrShelfStop(VirtualHandset.MotorGroup.MotorGroup1, true) < 0)
            {
                MessageBox.Show("Could not set container- or shelf-stop position!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            this.Enabled = true;
        }

        #endregion

    }
}
