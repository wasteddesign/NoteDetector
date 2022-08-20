using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Buzz.MachineInterface;
using BuzzGUI.Interfaces;
using BuzzGUI.Common;
using Pitch;
using System.Collections;

namespace WDE.NoteDetector
{
    [MachineDecl(Name = "Note Detector", ShortName = "NoteDet", Author = "WDE", MaxTracks = 1)]
	public class NoteDetector : IBuzzMachine, INotifyPropertyChanged
	{
		IBuzzMachineHost host;
        private PitchTracker m_pitchTracker;
        static readonly int SemitoneMid = 63;

        int prevNote = 0;
        int prevNoteWithSemi = 0;
        public string[] selMacTable = { };

        Random rnd = new Random();

        public NoteDetector(IBuzzMachineHost host)
		{
			this.host = host;            

            detectLevelThreshold = 7000;

            m_pitchTracker = new PitchTracker();
            m_pitchTracker.PitchRecordHistorySize = 100;
            m_pitchTracker.DetectLevelThreshold = 0.7f;            
            m_pitchTracker.SampleRate = Global.Buzz.SelectedAudioDriverSampleRate;
            m_pitchTracker.PitchDetected += M_pitchTracker_PitchDetected;

            //string notes = "";
            //foreach (var name in BuzzNote.Names)
            //{
            //    notes += "\"" + name + "\", ";
            //}
            //Global.Buzz.DCWriteLine(notes);
        }

        private void M_pitchTracker_PitchDetected(PitchTracker sender, PitchTracker.PitchRecord pitchRecord)
        {
            if (pitchRecord.MidiNote != 0 && prevNote != pitchRecord.MidiNote && pitchRecord.MidiNote >= NoteLow && pitchRecord.MidiNote <= NoteHigh)
            {   
                machineState.Text = Pitch.PitchDsp.GetNoteName(pitchRecord.MidiNote + 12, true, true);
                if (SendNotes)
                {
                    SendNoteToTargetMachines(MIDIChannel, prevNoteWithSemi, 0);
                    
                    int rndNum = rnd.Next(-VolRandom/2, VolRandom/2);
                    int vol = SendVelocity + rndNum;
                    vol = vol < 0 ? 0 : vol;
                    vol = vol > 127 ? 127 : vol;

                    int noteWithSemi = pitchRecord.MidiNote + Semitone - SemitoneMid;
                    noteWithSemi = noteWithSemi < 0 ? 0 : noteWithSemi;                    

                    SendNoteToTargetMachines(MIDIChannel, noteWithSemi, vol);
                    prevNoteWithSemi = noteWithSemi;
                }
                prevNote = pitchRecord.MidiNote;
            }
        }             

		[ParameterDecl(ValueDescriptions = new[] { "No", "Yes" }, Description = "Enable/Disable Note Detection.")]
		public bool Disable { get; set; }

        [ParameterDecl(ValueDescriptions = new[] { "No", "Yes" }, Description = "Enable/Disable sending notes.")]
        public bool SendNotes { get; set; }

        [ParameterDecl(DefValue = 0, MinValue = 0, MaxValue = 15, Description = "Send MIDI Note Channel")]
        public int MIDIChannel { get; set; }

        [ParameterDecl(DefValue = 60, MinValue = 0, MaxValue = 127, Description = "Send Velocity")]
        public int SendVelocity { get; set; }

        [ParameterDecl(DefValue = 0, MinValue = 0, MaxValue = 127, Description = "Randomize Volume")]
        public int VolRandom { get; set; }

        [ParameterDecl(DefValue = 63, MinValue = 0, MaxValue = 127, Description = "Raise or Lower the note to send by X semitones.")]
        public int Semitone { get; set; }

        int detectLevelThreshold;
        [ParameterDecl(DefValue = 7000, MinValue = 0, MaxValue = 10000, Description = "Note Detection Level Threshold.")]
        public int DetectLevelThreshold { get { return detectLevelThreshold; }
            set
            {
                detectLevelThreshold = value;                
                m_pitchTracker.DetectLevelThreshold = ((float)detectLevelThreshold) / 10000.0f;
                m_pitchTracker.Reset();
            }
        }
               
        [ParameterDecl(DefValue = 3200, MinValue = 0, MaxValue = 10000, Description = "Note release sensitivity when input volume goes down.")]
        public int NoteReleaseThreshold { get; set; }

        [ParameterDecl(DefValue = true, ValueDescriptions = new[] { "No", "Yes" }, Description = "Enable/Disable sending note off.")]
        public bool SendNoteOff { get; set; }

        [ParameterDecl(DefValue = 0, MaxValue = (BuzzNote.MaxOctave + 1) * 12 - 1, MinValue = 0, ValueDescriptions = new string[] { "C-0", "C#0", "D-0", "D#0", "E-0", "F-0", "F#0", "G-0", "G#0", "A-0", "A#0", "B-0", "C-1", "C#1", "D-1", "D#1", "E-1", "F-1", "F#1", "G-1", "G#1", "A-1", "A#1", "B-1", "C-2", "C#2", "D-2", "D#2", "E-2", "F-2", "F#2", "G-2", "G#2", "A-2", "A#2", "B-2", "C-3", "C#3", "D-3", "D#3", "E-3", "F-3", "F#3", "G-3", "G#3", "A-3", "A#3", "B-3", "C-4", "C#4", "D-4", "D#4", "E-4", "F-4", "F#4", "G-4", "G#4", "A-4", "A#4", "B-4", "C-5", "C#5", "D-5", "D#5", "E-5", "F-5", "F#5", "G-5", "G#5", "A-5", "A#5", "B-5", "C-6", "C#6", "D-6", "D#6", "E-6", "F-6", "F#6", "G-6", "G#6", "A-6", "A#6", "B-6", "C-7", "C#7", "D-7", "D#7", "E-7", "F-7", "F#7", "G-7", "G#7", "A-7", "A#7", "B-7", "C-8", "C#8", "D-8", "D#8", "E-8", "F-8", "F#8", "G-8", "G#8", "A-8", "A#8", "B-8", "C-9", "C#9", "D-9", "D#9", "E-9", "F-9", "F#9", "G-9", "G#9", "A-9", "A#9", "B-9" }, Description = "Lowest Note to send.")]
        public int NoteLow { get; set; }

        [ParameterDecl(DefValue = (BuzzNote.MaxOctave + 1) * 12 - 1, MaxValue = (BuzzNote.MaxOctave + 1) * 12 - 1, MinValue = 0, ValueDescriptions = new string[] { "C-0", "C#0", "D-0", "D#0", "E-0", "F-0", "F#0", "G-0", "G#0", "A-0", "A#0", "B-0", "C-1", "C#1", "D-1", "D#1", "E-1", "F-1", "F#1", "G-1", "G#1", "A-1", "A#1", "B-1", "C-2", "C#2", "D-2", "D#2", "E-2", "F-2", "F#2", "G-2", "G#2", "A-2", "A#2", "B-2", "C-3", "C#3", "D-3", "D#3", "E-3", "F-3", "F#3", "G-3", "G#3", "A-3", "A#3", "B-3", "C-4", "C#4", "D-4", "D#4", "E-4", "F-4", "F#4", "G-4", "G#4", "A-4", "A#4", "B-4", "C-5", "C#5", "D-5", "D#5", "E-5", "F-5", "F#5", "G-5", "G#5", "A-5", "A#5", "B-5", "C-6", "C#6", "D-6", "D#6", "E-6", "F-6", "F#6", "G-6", "G#6", "A-6", "A#6", "B-6", "C-7", "C#7", "D-7", "D#7", "E-7", "F-7", "F#7", "G-7", "G#7", "A-7", "A#7", "B-7", "C-8", "C#8", "D-8", "D#8", "E-8", "F-8", "F#8", "G-8", "G#8", "A-8", "A#8", "B-8", "C-9", "C#9", "D-9", "D#9", "E-9", "F-9", "F#9", "G-9", "G#9", "A-9", "A#9", "B-9" }, Description = "Highest Note to send.")]
        public int NoteHigh { get; set; }

        public bool Work(Sample[] output, Sample[] input, int numsamples, WorkModes mode)
        {
            float[] workBuf = new float[numsamples];

            bool silentinput = true;
            float epsilon = (float)NoteReleaseThreshold / 100.0f;

            for (int i = 0; i < numsamples; i++)
            {
                if (input[i].L > epsilon || input[i].L < -epsilon || input[i].R > epsilon || input[i].R < -epsilon)
                {
                    silentinput = false;
                    break;
                }
            }

            if (silentinput && SendNotes && prevNoteWithSemi != 0)
            {                
                if (SendNoteOff)
                    SendNoteToTargetMachines(MIDIChannel, prevNoteWithSemi, 0);

                prevNoteWithSemi = 0;             
            }
        
            if (mode == WorkModes.WM_READWRITE)
            {
                if (!Disable)
                {
                    for (int i = 0; i < numsamples; i++)
                        workBuf[i] = (input[i].L + input[i].R) / 2.0f;
                    m_pitchTracker.ProcessBuffer(workBuf, numsamples);
                }
                for (int i = 0; i < numsamples; i++)
                {
                    output[i] = input[i];
                }
                return true;
            }
            
            return false;
        }
		
		// actual machine ends here. the stuff below demonstrates some other features of the api.	
		public class State : INotifyPropertyChanged
		{
			public State() { text = "C 4"; }	// NOTE: parameterless constructor is required by the xml serializer

			string text;
			public string Text 
			{
				get { return text; }
				set
				{
					text = value;
					if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Text"));
					// NOTE: the INotifyPropertyChanged stuff is only used for data binding in the GUI in this demo. it is not required by the serializer.
				}
			}

            string selectedMachines = "";
            public string SelectedMachines
            {
                get
                {                    
                    return selectedMachines;
                }
                set
                {
                    selectedMachines = value;                    
                }
            }            

			public event PropertyChangedEventHandler PropertyChanged;
		}

		State machineState = new State();
		public State MachineState			// a property called 'MachineState' gets automatically saved in songs and presets
		{
			get { return machineState; }
			set
			{
				machineState = value;
				if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("MachineState"));
                string selMac = machineState.SelectedMachines;
                selMacTable = selMac.Split('\n');
            }
		}		
		
		public IEnumerable<IMenuItem> Commands
		{
			get
			{				
				yield return new MenuItemVM() 
				{ 
					Text = "About...", 
					Command = new SimpleCommand()
					{
						CanExecuteDelegate = p => true,
						ExecuteDelegate = p => MessageBox.Show("Note Detector 0.7\n\nUsing Pitch Tracker https://pitchtracker.codeplex.com/ \nhttps://pitchtracker.codeplex.com/license \n\n(C) 2017 WDE")
					}
				};
			}
		}
        
        public void SendNoteToTargetMachines(int channel, int note, int velocity)
        {
            foreach (string item in this.selMacTable)
            {
                IMachine machine = Global.Buzz.Song.Machines.FirstOrDefault(m => m.Name == item);

                if (machine != null)
                    machine.SendMIDINote(channel, note, velocity);
            }
            // Global.Buzz.DCWriteLine(DateTime.Now.Second + "Channel: " + channel + ", Note: " + note + ", Vel: " + velocity);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void TargetMachinesChanged(IList selectedItems)
        {            
            string targetMachines = "";
            
            Object thisLock = new Object();
            lock (thisLock)
            {
                selMacTable = new string[selectedItems.Count];
                int i = 0;

                foreach (var item in selectedItems)
                {                    
                    selMacTable[i++] = item.ToString();
                    targetMachines += item.ToString() + "\n";
                }
                char[] rem = { '\n' };
                targetMachines = targetMachines.TrimEnd(rem);
                machineState.SelectedMachines = targetMachines;
            }            
        }
    }

	public class MachineGUIFactory : IMachineGUIFactory { public IMachineGUI CreateGUI(IMachineGUIHost host) { return new PitchGUI(); } }
    public class PitchGUI : UserControl, IMachineGUI
    {
        IMachine machine;
        NoteDetector pitchMachine;
        TextBox tb;
        ListBox lb;

        // view model for machine list box items
        public class MachineVM
        {
            public IMachine Machine { get; private set; }
            public MachineVM(IMachine m) { Machine = m; }
            public override string ToString() { return Machine.Name; }
        }

        public ObservableCollection<MachineVM> Machines { get; private set; }

        public IMachine Machine
        {
            get { return machine; }
            set
            {
                if (machine != null)
                {
                    BindingOperations.ClearBinding(tb, TextBox.TextProperty);
                    machine.Graph.MachineAdded -= machine_Graph_MachineAdded;
                    machine.Graph.MachineRemoved -= machine_Graph_MachineRemoved;
                }

                machine = value;

                if (machine != null)
                {
                    pitchMachine = (NoteDetector)machine.ManagedMachine;
                    tb.SetBinding(TextBox.TextProperty, new Binding("MachineState.Text") { Source = pitchMachine, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

                    machine.Graph.MachineAdded += machine_Graph_MachineAdded;
                    machine.Graph.MachineRemoved += machine_Graph_MachineRemoved;

                    foreach (var m in machine.Graph.Machines)
                        machine_Graph_MachineAdded(m);

                    lb.SetBinding(ListBox.ItemsSourceProperty, new Binding("Machines") { Source = this, Mode = BindingMode.OneWay });

                    Object thisLock = new Object();

                    string[] machines_ = pitchMachine.selMacTable;
                    foreach (var item in machines_)
                    {
                        foreach (var lbitem in lb.Items)
                        {
                            if (item.ToString() == lbitem.ToString())
                                lb.SelectedItems.Add(lbitem);
                        }
                    }
                }
            }
        }

        void machine_Graph_MachineAdded(IMachine machine)
        {
            Machines.Add(new MachineVM(machine));
        }

        void machine_Graph_MachineRemoved(IMachine machine)
        {
            Machines.Remove(Machines.First(m => m.Machine == machine));
        }

        public PitchGUI()
        {
            Machines = new ObservableCollection<MachineVM>();

            tb = new TextBox() { Margin = new Thickness(0, 0, 0, 4), AllowDrop = true };
            tb.TextAlignment = TextAlignment.Center;
            tb.FontSize = 100;
            tb.IsReadOnly = true;
            lb = new ListBox() { Height = 100, Margin = new Thickness(0, 0, 0, 4) };
            lb.SelectionChanged += Lb_SelectionChanged;
            lb.SelectionMode = SelectionMode.Multiple;
            Label label = new Label();
            label.Content = "MIDI Send Targets:";
            var sp = new StackPanel();
            sp.Children.Add(tb);
            sp.Children.Add(label);
            sp.Children.Add(lb);
            this.Content = sp;
        }

        private void Lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lbs = (ListBox)sender;
            pitchMachine = (NoteDetector)machine.ManagedMachine;
            pitchMachine.TargetMachinesChanged(lbs.SelectedItems);
        }
    }

}
