using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel81Plant
    {
        public const float Step=.025f;
        public const int Words=256;
        public enum Ecc { Clean, Corrected, Double }
        public enum Packet { Accepted, Corrupt, Duplicate, Replay, Gap, Expired }
        public struct Codeword
        {
            public ulong Low;
            public byte High;

            public bool Get(int bit)=>bit<64?(Low&(1UL<<bit))!=0:(High&(1<<(bit-64)))!=0;
            public void Flip(int bit)
            {
                if(bit<64)Low^=1UL<<bit;
                else High^=(byte)(1<<(bit-64));
            }
        }
        public readonly struct Frame
        {
            public readonly uint Sequence,Payload,Time,Check;
            public Frame(uint sequence,uint payload,uint time)
            {
                Sequence=sequence;Payload=payload;Time=time;
                Check=Checksum(new[]{sequence,payload,time});
            }
            private Frame(uint sequence,uint payload,uint time,uint check)
            {Sequence=sequence;Payload=payload;Time=time;Check=check;}
            public Frame Corrupt()=>new(Sequence,Payload^0x200,Time,Check);
        }
        private readonly Codeword[] memory=new Codeword[Words];
        private readonly ulong[] vault=new ulong[Words];
        private readonly bool[] quarantined=new bool[Words/16];
        private readonly ulong[] lanes=new ulong[3];
        private readonly float[] laneTime=new float[3];
        private uint vaultCrc,expectedSequence=1,lastPacketTick,noise=0x996FE8u;
        private Frame lastFrame,previousFrame;
        private int scrubAddress;
        public int Inspected {get;private set;}
        public int Corrections {get;private set;}
        public int DoubleErrors {get;private set;}
        public int QuarantineCount {get;private set;}
        public int LastSyndrome {get;private set;}
        public int FailedLane {get;private set;}=-1;
        public ulong Vote {get;private set;}
        public ulong Reference {get;private set;}
        public bool VoterValid {get;private set;}
        public int CorruptFrames {get;private set;}
        public int Replays {get;private set;}
        public int Duplicates {get;private set;}
        public int Gaps {get;private set;}
        public int Retries {get;private set;}
        public int ExpiredFrames {get;private set;}
        public int Committed {get;private set;}
        public int Blocked {get;private set;}
        public Packet LastPacket {get;private set;}=Packet.Expired;
        public bool Initialized {get;private set;}
        public bool SingleVerified {get;private set;}
        public bool CheckpointVerified {get;private set;}
        public bool Watchdog {get;private set;}
        public bool PacketFresh(uint now)=>expectedSequence>1&&now-lastPacketTick<55;
        public float Recovery {get;set;}
        public float Storm {get;set;}

        private static bool ParityPosition(int position)=>(position&(position-1))==0;
        public static Codeword Encode(ulong original)
        {
            Codeword c=default;
            int data=0, syndrome=0, parity=0;
            for(int pos=1;pos<=71;pos++)
            {
                if(ParityPosition(pos))continue;
                if((original&(1UL<<data++))==0)continue;
                c.Flip(pos-1);syndrome^=pos;parity^=1;
            }
            for(int i=1;i<=64;i*=2)if((syndrome&i)!=0){c.Flip(i-1);parity^=1;}
            if(parity!=0)c.Flip(71);
            return c;
        }
        public static Ecc Decode(ref Codeword c,out ulong data,out int syndrome)
        {
            data=0;syndrome=0;
            bool odd=c.Get(71);
            for(int pos=1;pos<=71;pos++)
            {
                if(!c.Get(pos-1))continue;
                syndrome^=pos;odd=!odd;
            }
            if(!odd&&syndrome!=0||odd&&syndrome>71)return Ecc.Double;
            if(odd)c.Flip(syndrome==0?71:syndrome-1);
            int bit=0;
            for(int pos=1;pos<=71;pos++)
                if(!ParityPosition(pos))
                {
                    if(c.Get(pos-1))data|=1UL<<bit;
                    bit++;
                }
            return odd?Ecc.Corrected:Ecc.Clean;
        }
        public void Initialize()
        {
            for(int i=0;i<Words;i++)
            {
                vault[i]=0x30121BCA45002UL+0xA73CF2BUL*(ulong)(i+1);
                memory[i]=Encode(vault[i]);
            }
            Array.Clear(quarantined,0,quarantined.Length);
            vaultCrc=VaultChecksum();QuarantineCount=0;Initialized=true;
        }
        public void Inject(bool pair)
        {
            // First two addresses belong to different physical banks.
            int address=pair?128:0;
            scrubAddress=address;
            noise^=noise<<13;noise^=noise>>17;noise^=noise<<5;
            int bit=(int)(noise%72);
            memory[address].Flip(bit);
            if(pair)memory[address].Flip((bit+23)%72);
            Storm=1;
        }
        public bool Read(int address,out ulong value)
        {
            value=0;
            if(!Initialized||quarantined[address/16])return false;
            Codeword code=memory[address];
            Ecc status=Decode(ref code,out value,out int s);
            if(status==Ecc.Clean)return true;
            LastSyndrome=s;
            if(status==Ecc.Double)
            {
                DoubleErrors++;quarantined[address/16]=true;QuarantineCount++;
                value=0;return false;
            }
            if(Decode(ref code,out ulong verified,out _)!=Ecc.Clean||verified!=value)return false;
            memory[address]=code;Corrections++;
            if(address==0&&vault[0]==verified)SingleVerified=true;
            return true;
        }
        public void Scrub(int slots)
        {
            for(int i=0;i<slots;i++)
            {
                Read(scrubAddress,out _);Inspected++;
                scrubAddress=(scrubAddress+1)%Words;
            }
        }
        public bool ValidateMemory()
        {
            if(!Initialized||QuarantineCount!=0)return false;
            for(int i=0;i<Words;i++)
                if(!Read(i,out ulong value)||value!=vault[i])return false;
            return true;
        }
        public bool RestoreVault()
        {
            // The private vault is initialized once for this mission and cannot be written by the fault injector.
            if(!Initialized||vaultCrc!=VaultChecksum())return false;
            for(int page=0;page<quarantined.Length;page++)
            {
                if(!quarantined[page])continue;
                for(int i=page*16;i<(page+1)*16;i++)memory[i]=Encode(vault[i]);
                for(int i=page*16;i<(page+1)*16;i++)
                    if(Decode(ref memory[i],out ulong value,out _)!=Ecc.Clean||value!=vault[i])return false;
                quarantined[page]=false;QuarantineCount--;
            }
            CheckpointVerified=ValidateMemory();return CheckpointVerified;
        }
        private uint VaultChecksum()
        {
            var words=new uint[Words*2+2];
            words[0]=0x4D495331;words[1]=1; // Mission identity and immutable checkpoint version.
            for(int i=0;i<Words;i++){words[i*2+2]=(uint)vault[i];words[i*2+3]=(uint)(vault[i]>>32);}
            return Checksum(words);
        }
        public static uint Checksum(uint[] values)
        {
            uint crc=uint.MaxValue;
            foreach(uint value in values)
                for(int b=0;b<32;b++)
                {
                    uint carry=(crc^(value>>b))&1;
                    crc=(crc>>1)^(carry==0?0:0xEDB88320u);
                }
            return ~crc;
        }
        public void StartComputation()
        {
            Array.Clear(laneTime,0,3);VoterValid=false;
        }
        public bool Compute(int phase,float dt,int faultLanes)
        {
            int address=(phase*31)%128;
            if(!Read(address,out ulong a)||!Read(address+128,out ulong b)){VoterValid=false;return false;}
            int mask=0;
            for(int i=0;i<3;i++)
            {
                laneTime[i]+=dt*(1f-i*.035f);
                if(laneTime[i]<.38f)continue;
                lanes[i]=unchecked(a*13+b*7+(ulong)phase*41);
                if(i<faultLanes)lanes[i]^=0x010020UL;
                mask|=1<<i;
            }
            if(mask!=7)return false;
            Vote=(lanes[0]&lanes[1])|(lanes[0]&lanes[2])|(lanes[1]&lanes[2]);
            Reference=unchecked((a<<3)+(a<<2)+a+(b<<3)-b+(ulong)phase*40+(ulong)phase);
            FailedLane=lanes[0]!=Reference?0:lanes[1]!=Reference?1:lanes[2]!=Reference?2:-1;
            VoterValid=Vote==Reference;
            return true;
        }
        public bool CheckWatchdog(uint frame,uint answer,float age)
        {
            Watchdog=age<.60f&&answer==WatchdogReply(frame);
            return Watchdog;
        }
        public static uint WatchdogReply(uint frame)=>((frame<<5)|(frame>>27))^0xA7DC3021u;
        public Frame Make(uint now,uint offset=0)=>new(expectedSequence+offset,0xC831u^(expectedSequence+offset)*712u,now);
        public Packet Accept(Frame f,uint now)
        {
            if(f.Check!=Checksum(new[]{f.Sequence,f.Payload,f.Time})){CorruptFrames++;return LastPacket=Packet.Corrupt;}
            if(expectedSequence>1&&f.Sequence==expectedSequence-1){Duplicates++;return LastPacket=Packet.Duplicate;}
            if(f.Sequence<expectedSequence){Replays++;return LastPacket=Packet.Replay;}
            if(f.Sequence>expectedSequence){Gaps++;return LastPacket=Packet.Gap;}
            if(now-f.Time>=55){ExpiredFrames++;return LastPacket=Packet.Expired;}
            lastPacketTick=now;previousFrame=lastFrame;lastFrame=f;expectedSequence++;
            return LastPacket=Packet.Accepted;
        }
        public bool PacketDisturbance(uint now)
        {
            Frame packet=Make(now);
            bool bad=Accept(packet.Corrupt(),now)==Packet.Corrupt;
            bool gap=Accept(Make(now,1),now)==Packet.Gap;
            bool duplicate=Accept(lastFrame,now)==Packet.Duplicate;
            bool replay=Accept(previousFrame,now)==Packet.Replay;
            bool old=Accept(Make(now-60),now)==Packet.Expired;
            return bad&&gap&&duplicate&&replay&&old;
        }
        public bool Retransmit(uint now)
        {
            bool ok=Accept(Make(now),now)==Packet.Accepted;
            if(ok)Retries++;
            return ok;
        }
        public void RecordCommand(bool permit)
        {
            if(permit)Committed++;
            else Blocked++;
        }
        public void Cool(float dt){Storm=Mathf.MoveTowards(Storm,0,dt*.80f);Recovery=Mathf.MoveTowards(Recovery,0,dt*.65f);}
    }
}


