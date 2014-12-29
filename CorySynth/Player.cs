using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Midi;
using NAudio.Wave;

namespace CorySignalGenerator
{
    /// <summary>
    /// This delegate represents when there are new player events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="newEvents"></param>
    public delegate void NewPlayerEvents(Player sender, Queue<MidiEvent> newEvents);

    public delegate void PlayerSequenceEnded(Player sender);


    public class Player
    {
        private object _lock = new object();
        public Player(MidiEventCollection sequence)
        {
            Sequence = sequence;
            SequencePosition = 0;
            TicksToNext = 0;
        }

        public int Track { get { return 0; } }

        public int TicksToNext { get; private set; }

        public int SequencePosition { get; private set; }

        public MidiEventCollection Sequence { get; set; }

        public event PlayerSequenceEnded SequenceEnded;

        public event NewPlayerEvents NewMidiEvents;

        public void Reset()
        {
            TicksToNext = 0;
            SequencePosition = 0;
        }

        public void NextTick()
        {
            lock (_lock)
            {
                if(SequencePosition >= Sequence[Track].Count)
                {
                    Console.WriteLine("No more items in sequence!");
                    OnSequenceComplete();
                    return;
                }

                TicksToNext--;
                if (TicksToNext <= 0)
                {
                    Console.WriteLine("Time to get more stuff! {0}", SequencePosition);
                    TicksToNext = 0;
                    var events = GetMidiEvents();
                    OnNewEvents(events);
                    if (CurrentEvent != null)
                        TicksToNext = (int)CurrentEvent.AbsoluteTime;
                    else
                        OnSequenceComplete();
                }
            }
        }

        protected MidiEvent CurrentEvent
        {
            get
            {
                if(SequencePosition < Sequence[Track].Count)
                    return Sequence[Track][SequencePosition];
                return
                    null;
            }
        }

        protected Queue<MidiEvent> GetMidiEvents()
        {
            var queue = new Queue<MidiEvent>();

            Console.WriteLine("Enqueing {0}", CurrentEvent);
            queue.Enqueue(CurrentEvent);
            SequencePosition++;
            while(SequencePosition < Sequence[Track].Count && Sequence[Track][SequencePosition].AbsoluteTime==0){
                Console.WriteLine("Enqueing {0}", CurrentEvent);
                queue.Enqueue(CurrentEvent);
                SequencePosition++;
            }
            return queue;
            
        }

        private void OnNewEvents(Queue<MidiEvent> notes)
        {
            if (NewMidiEvents != null)
                NewMidiEvents(this, notes);
        }

        private void OnSequenceComplete()
        {
            if (SequenceEnded != null)
                SequenceEnded(this);

        }


    }
}
