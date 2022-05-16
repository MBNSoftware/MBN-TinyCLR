/*
 * Buzz Click board driver for TinyCLR 2.0
 * 
 * Version 1.0 :
 *   - Initial revision
 *   - Thanks to GHI for the great pieces on Tone, Melody, etc.
 *    
 * Copyright 2020 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License..
 */

#if (NANOFRAMEWORK_1_0)
using Windows.Devices.Pwm;
#else
using GHIElectronics.TinyCLR.Devices.Pwm;
#endif

using System;
using System.Threading;
using System.Collections;

namespace MBN.Modules
{
    /// <summary>
	/// Main class for Buzz module for Microbus.Net driver 
	/// </summary>
	/// <example> This sample shows a basic usage of the Buzzer Click board.
    /// <code language="C#">
	/// public class Program
    /// {
    ///     static BuzzerClick _buzzer;
    ///     
    ///     // This method is run when the mainboard is powered up or reset.   
    ///     public static void Main()
    ///     {
    ///         _buzzer = new BuzzerClick(Hardware.SocketOne);
    ///         var note = new BuzzerClick.MusicNote(BuzzerClick.Tone.C4, 400);
    /// 
    ///         _buzzer.AddNote(note);
    /// 
    ///         // up
    ///         PlayNote(BuzzerClick.Tone.C4);
    ///         PlayNote(BuzzerClick.Tone.D4);
    ///         PlayNote(BuzzerClick.Tone.E4);
    ///         PlayNote(BuzzerClick.Tone.F4);
    ///         PlayNote(BuzzerClick.Tone.G4);
    ///         PlayNote(BuzzerClick.Tone.A4);
    ///         PlayNote(BuzzerClick.Tone.B4);
    ///         PlayNote(BuzzerClick.Tone.C5);
    /// </code>
    /// </example>
    public sealed class BuzzerClick
    {
		// PWM channel for alternating the pulse at given frequencies.
#if (NANOFRAMEWORK_1_0)
		private readonly PwmPin _buzzPwm;
#else
		private readonly PwmChannel _buzzPwm;
#endif
		private readonly PwmController PWM;
		private Melody _playList;
		private Boolean _running;
		private Thread _playbackThread;

		/// <summary>
		/// Main class constructor for Buzzer 
		/// <para><b>Pins used :</b> Cs, Pwm</para>
		/// </summary>
		/// <param name="socket">The socket on which the Buzzer Click board is plugged on MikroBus.Net</param>
		/// <exception cref="System.InvalidOperationException">Thrown if some pins are already in use by another board on the same socket</exception>
		public BuzzerClick(Hardware.Socket socket)
		{
			// Initialize PWM and set initial brightness
#if (NANOFRAMEWORK_1_0)
			PWM = PwmController.FromId(socket.PwmController);
			PWM.SetDesiredFrequency(5000);
			_buzzPwm = PWM.OpenPin(socket.PwmPin);
			_buzzPwm.SetActiveDutyCyclePercentage(0.0);
#else
			PWM = PwmController.FromName(socket.PwmController);
			PWM.SetDesiredFrequency(10000);
			_buzzPwm = PWM.OpenChannel(socket.PwmChannel);
			_buzzPwm.SetActiveDutyCyclePercentage(0.0);
#endif
			_buzzPwm.Start();

			_playList = new Melody();
		}
            
#region Melody-stuff
		/// <summary>
		/// Represents a list of notes to play in sequence.
		/// </summary>
        /// <example> This sample shows how to use the Melody class.
        /// <code language="C#">
        /// var melody = new BuzzerClick.Melody();
        ///
        ///    // up
        ///    melody.Add(BuzzerClick.Tone.C4, 200);
        ///    melody.Add(BuzzerClick.Tone.D4, 200);
        ///    melody.Add(BuzzerClick.Tone.E4, 200);
        ///    melody.Add(BuzzerClick.Tone.F4, 200);
        ///    melody.Add(BuzzerClick.Tone.E4, 200);
        ///    melody.Add(BuzzerClick.Tone.C4, 200);
        ///
        ///    _buzzer.Play(melody);
        /// </code>
        /// </example>
		public class Melody
		{
			private readonly Queue _list;

			/// <summary>
			/// Creates a new instance of a melody.
			/// </summary>
			/// <example> This sample shows how to use the Melody.Melody() method.
			/// <code language="C#">
			/// var melody = new BuzzerClick.Melody();
			/// melody.Add(BuzzerClick.Tone.C4, 200);
			/// </code>
			/// </example>
			public Melody() => _list = new Queue();

			/// <summary>
			/// Adds a new note to the list to play.
			/// </summary>
			/// <param name="frequency">The frequency of the note.</param>
			/// <param name="milliseconds">The duration of the note.</param>
			/// <example> This sample shows how to use the Melody.Add() method.
			/// <code language="C#">
			/// var melody = new BuzzerClick.Melody();
			/// melody.Add(12000, 200);
			/// </code>
			/// </example>
			public void Add(Int32 frequency, Int32 milliseconds) => Add(new Tone(frequency), milliseconds);

			/// <summary>
			/// Adds a new note to the list to play.
			/// </summary>
			/// <param name="tone">The tone of the note.</param>
			/// <param name="duration">The duration of the note.</param>
			/// <example> This sample shows how to use the Melody.Add() method.
			/// <code language="C#">
			/// var melody = new BuzzerClick.Melody();
			/// melody.Add(BuzzerClick.Tone.C4, 200);
			/// </code>
			/// </example>
			public void Add(Tone tone, Int32 duration) => Add(new MusicNote(tone, duration));

			/// <summary>
			/// Adds an existing note to the list to play.
			/// </summary>
			/// <param name="note">The note to add.</param>
			public void Add(MusicNote note) => _list.Enqueue(note);

			/// <summary>
			/// Gets the next note to play from the melody.
			/// </summary>
			/// <returns></returns>
			public MusicNote GetNextNote()
			{
				if (_list.Count == 0)
					throw new Exception("No notes added.");

				return (MusicNote)_list.Dequeue();
			}

			/// <summary>
			/// Gets the number of notes left to play in the melody.
			/// </summary>
			public Int32 NotesRemaining
			{
				get
				{
					return _list.Count;
				}
			}

			/// <summary>
			/// Removes all notes from the melody.
			/// </summary>
			public void Clear() => _list.Clear();
		}

		/// <summary>
		/// Class that holds and manages notes that can be played.
		/// </summary>
		public class Tone
		{
			/// <summary>
			/// Frequency of the note in hertz
			/// </summary>
			public Double Freq;

			/// <summary>
			/// Constructs a new Tone.
			/// </summary>
			/// <param name="freq">The frequency of the tone.</param>
			public Tone(Double freq) => Freq = freq;

			/// <summary>
			/// A "rest" note, or a silent note.
			/// </summary>
			public static readonly Tone Rest = new Tone(0.0);

#region 4th Octave
			/// <summary>
			/// C in the 4th octave. Middle C.
			/// </summary>
			public static readonly Tone C4 = new Tone(261.626);

			/// <summary>
			/// D in the 4th octave.
			/// </summary>
			public static readonly Tone D4 = new Tone(293.665);

			/// <summary>
			/// E in the 4th octave.
			/// </summary>
			public static readonly Tone E4 = new Tone(329.628);

			/// <summary>
			/// F in the 4th octave.
			/// </summary>
			public static readonly Tone F4 = new Tone(349.228);

			/// <summary>
			/// G in the 4th octave.
			/// </summary>
			public static readonly Tone G4 = new Tone(391.995);

			/// <summary>
			/// A in the 4th octave.
			/// </summary>
			public static readonly Tone A4 = new Tone(440);

			/// <summary>
			/// B in the 4th octave.
			/// </summary>
			public static readonly Tone B4 = new Tone(493.883);

#endregion 4th Octave

#region 5th Octave

			/// <summary>
			/// C in the 5th octave.
			/// </summary>
			public static readonly Tone C5 = new Tone(523.251);

#endregion 5th Octave
		}

		/// <summary>
		/// Class that describes a musical note, containing a tone and a duration.
		/// </summary>
		public class MusicNote
		{
			/// <summary>
			/// The tone of the note.
			/// </summary>
			public Tone Tone;
			/// <summary>
			/// The duration of the note.
			/// </summary>
			public Int32 Duration;

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="tone">The tone of the note.</param>
			/// <param name="duration">The duration that the note should be played.</param>
			public MusicNote(Tone tone, Int32 duration)
			{
				Tone = tone;
				Duration = duration;
			}
		}
#endregion

        /// <summary>
		/// Plays the given frequency indefinitely.
		/// </summary>
		/// <param name="frequency">The frequency to play.</param>
		public void Play(Int32 frequency)
		{
			_playList.Clear();
			_playList.Add(frequency, Int32.MaxValue);
			Play();
		}

		/// <summary>
		/// Plays the given tone indefinitely.
		/// </summary>
		/// <param name="tone">The tone to play.</param>
		public void Play(Tone tone) => Play((Int32)tone.Freq);

		/// <summary>
		/// Plays the melody.
		/// </summary>
		/// <param name="melody">The melody to play.</param>
		public void Play(Melody melody)
		{
			_playList = melody;
			Play();
		}

		/// <summary>
		/// Starts note playback of the notes added using AddNote(). Returns if it made any change.
		/// </summary>
		/// <returns>Returns true if notes were not playing and they were started. False if notes were already being played.</returns>
		public Boolean Play()
		{
            if (_running)
                Stop();

			// Make sure the queue is not empty and we are not currently playing it.
			if (_playList.NotesRemaining > 0)
			{
				_running = true;

				_playbackThread = new Thread(PlaybackThread);
				_playbackThread.Start();
			}

			return true;
		}

		private void PlaybackThread()
		{
			while (_running && _playList.NotesRemaining > 0)
			{
				// Get the next note.
				MusicNote currNote = _playList.GetNextNote();

				// Set the tone and sleep for the duration
				SetTone(currNote.Tone);

				Thread.Sleep(currNote.Duration);
			}

			SetTone(Tone.Rest);

			_running = false;
		}

		private void SetTone(Tone tone)
		{
            _buzzPwm.Stop();
            if (Math.Abs(tone.Freq) < Double.Epsilon)
			{
				return;
			}
			PWM.SetDesiredFrequency((Int32)tone.Freq);
			_buzzPwm.SetActiveDutyCyclePercentage(0.5);
			_buzzPwm.Start();
		}

		/// <summary>
		/// Stops note playback. Returns if it made any change.
		/// </summary>
		public void Stop()
		{
            if (_playbackThread != null)
            {
                _playbackThread.Abort();
                _playbackThread = null;
            }

            _running = false;
            _buzzPwm.Stop();
		}

		/// <summary>
		/// Adds a note to the queue to be played
		/// </summary>
		/// <param name="note">The note to be added, which describes the tone and duration to be played.</param>
		public void AddNote(MusicNote note) => _playList.Add(note);
	}
}


