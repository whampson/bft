using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WHampson.Cascara;

namespace WHampson.CascaraExamples
{
    class Program
    {
        const string LayoutXml = @"
<cascaraLayout name='a layout' description='A Test Layout'>
    <struct name='test'>
        <int name='foo'/>
        <struct name='nest'>
            <int name='bar'/>
            <byte name='abc' count='2'/>
            <char name='str' count='4'/>
            <echo message='${__OFFSET__}'/>
            <echo message='${__GLOBALOFFSET__}'/>
        </struct>
    </struct>
    <echo message='$OffsetOf(test.nest.str)'/>
    <echo message='$GlobalOffsetOf(test.nest.str)'/>
</cascaraLayout>";

        static void Main(string[] args)
        {
        //     byte[] data = { 0xBE, 0xBA, 0xFE, 0xCA, 0xEF, 0xBE, 0xAD, 0xDE, 0xAA, 0x55, 0x61, 0x62 ,0x63, 0x00 };

        //     LayoutScript layout = LayoutScript.Parse(LayoutXml);
        //     BinaryFile file = new BinaryFile(data);
        //     file.ApplyLayout(layout);

        //     Console.WriteLine("{0}: {1}", nameof(LayoutScript), layout);
        //     Console.WriteLine("{0}: {1}", nameof(BinaryFile), file);

            Deserialize();
        }

        static void Deserialize()
        {
            // PlayerData:
            //  0000    short       Health              1000
            //  0002    short       Armor               767
            //  0004    int         Score               564353
            //  -       Weapon[4]   Weapons
            //  0008    uint        Weapons[0].Id       0
            //  000C    uint        Weapons[0].Ammo     16116
            //  0010    uint        Weapons[1].Id       1
            //  0014    uint        Weapons[1].Ammo     85
            //  0018    uint        Weapons[2].Id       2
            //  001C    uint        Weapons[2].Ammo     77172
            //  0020    uint        Weapons[3].Id       3
            //  0024    uint        Weapons[3].Ammo     12378
            //  0028    long        Seed                547546237654745238
            //  -       Statistics  Stats
            //  0030    uint        Stats.NumKills      984
            //  0034    uint        Stats.NumDeaths     1
            //  0038    int         Stats.HighScore     564353
            //  003C    bool        Stats.FoundTreasure True
            //  003D    byte[3]     (align)
            //  0040    Char8[32]   Name                "Tiberius"
            byte[] data = new byte[]
            {
                0xE8, 0x03, 0xFF, 0x02, 0x81, 0x9C, 0x08, 0x00,     // Health, Armor, Score
                0x00, 0x00, 0x00, 0x00, 0xF4, 0x3E, 0x00, 0x00,     // Weapon[0]
                0x01, 0x00, 0x00, 0x00, 0x55, 0x00, 0x00, 0x00,     // Weapon[1]
                0x02, 0x00, 0x00, 0x00, 0x74, 0x2D, 0x01, 0x00,     // Weapon[2]
                0x03, 0x00, 0x00, 0x00, 0x5A, 0x30, 0x00, 0x00,     // Weapon[3]
                0x96, 0x40, 0x83, 0xF1, 0x66, 0x46, 0x99, 0x07,     // Seed
                0xD8, 0x03, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,     // Stats
                0x81, 0x9C, 0x08, 0x00, 0x01, 0x00, 0x00, 0x00,
                0x54, 0x69, 0x62, 0x65, 0x72, 0x69, 0x75, 0x73,     // Name
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            string src = @"
                <cascaraLayout>
                    <short name='Health'/>
                    <short name='Armor'/>
                    <int name='Score'/>
                    <struct name='Weapons' count='4'>
                        <uint name='Id'/>
                        <uint name='Ammo'/>
                    </struct>
                    <long name='Seed'/>
                    <struct name='Stats'>
                        <uint name='NumKills'/>
                        <uint name='NumDeaths'/>
                        <int name='HighScore'/>
                        <bool name='FoundTreasure'/>
                        <align count='3'/>
                    </struct>
                    <char name='Name' count='32'/>
                </cascaraLayout>
            ";

            BinaryFile b = new BinaryFile(data);
            b.ApplyLayout(LayoutScript.Parse(src));
            DeserializationFlags flags = DeserializationFlags.Fields
                | DeserializationFlags.IgnoreCase
                | DeserializationFlags.NonPublic;
            PlayerData3 p = b.Deserialize<PlayerData3>(flags);

            Console.WriteLine(b.Get<int>(0x38));
            Console.WriteLine(p.Stats.HighScore);
            p.Stats.HighScore = 1234;
            Console.WriteLine(p.Stats.HighScore);
            Console.WriteLine(b.Get<int>(0x38));
            p.Name = "The Quick Brown Fox jumps over the Lazy Dog";
            Console.WriteLine(p.Name);
        }

        class PlayerData1
        {
            public short Health { get; set; }
            public short Armor { get; set; }
            public int Score { get; set; }
            public Weapon1[] Weapons { get; }
            public long Seed { get; set; }
            public Statistics1 Stats { get; }
            public Char8[] Name { get; }
        }

        class Weapon1
        {
            public uint Id { get; set; }
            public uint Ammo { get; set; }
        }

        class Statistics1
        {
            public uint NumKills { get; set; }
            public uint NumDeaths { get; set; }
            public int HighScore { get; set; }
            public bool FoundTreasure { get; set; }
        }

        class PlayerData2
        {
            public Primitive<short> Health { get; set; }
            public Primitive<short> Armor { get; set; }
            public Primitive<int> Score { get; set; }
            public Weapon2[] Weapons { get; set; }
            public Primitive<long> Seed { get; set; }
            public Statistics2 Stats { get; set; }
            public Primitive<Char8> Name { get; set; }
        }

        class Weapon2
        {
            public Primitive<uint> Id { get; set; }
            public Primitive<uint> Ammo { get; set; }
        }

        class Statistics2
        {
            public Primitive<uint> NumKills { get; set; }
            public Primitive<uint> NumDeaths { get; set; }
            public Primitive<int> HighScore { get; set; }
            public Primitive<bool> FoundTreasure { get; set; }
        }

        class PlayerData3
        {
            private Primitive<short> health;
            private Primitive<short> armor;
            private Primitive<int> score;
            private Weapon3[] weapons;
            private Primitive<long> seed;
            private Statistics3 stats;
            private Primitive<Char8> name;

            public short Health
            {
                get { return health.Value; }
                set { health.Value = value; }
            }

            public short Armor
            {
                get { return armor.Value; }
                set { armor.Value = value; }
            }

            public int Score
            {
                get { return score.Value; }
                set { score.Value = value; }
            }

            public Weapon3[] Weapons
            {
                get { return weapons; }
            }

            public long Seed
            {
                get { return seed.Value; }
                set { seed.Value = value; }
            }

            public Statistics3 Stats
            {
                get { return stats; }
            }

            public string Name
            {
                get { return name.StringValue; }
                set
                {
                    // Char8 c;
                    // int i = 0;
                    // while ((c = value[i]) != '\0' && i < name.Length)
                    // {
                    //     name[i++].Value = c;
                    // }
                    // if (i < name.Length)
                    // {
                    //     name[i].Value = '\0';
                    // }

                    int i = 0;
                    while (i < value.Length && i < name.Length)
                    {
                        name[i].Value = value[i];
                        i++;
                    }
                    if (i < name.Length)
                    {
                        name[i].Value = '\0';
                    }
                }
            }
        }

        class Weapon3
        {
            private Primitive<uint> id;
            private Primitive<uint> ammo;

            public uint Id
            {
                get { return id.Value; }
                set { id.Value = value; }
            }

            public uint Ammo
            {
                get { return ammo.Value; }
                set { ammo.Value = value; }
            }
        }

        class Statistics3
        {
            private Primitive<uint> numKills;
            private Primitive<uint> numDeaths;
            private Primitive<int> highScore;
            private Primitive<bool> foundTreasure;

            public uint NumKills
            {
                get { return numKills.Value; }
                set { numKills.Value = value; }
            }

            public uint NumDeaths
            {
                get { return numDeaths.Value; }
                set { numDeaths.Value = value; }
            }

            public int HighScore
            {
                get { return highScore.Value; }
                set { highScore.Value = value; }
            }

            public bool FoundTreasure
            {
                get { return foundTreasure.Value; }
                set { foundTreasure.Value = value; }
            }
        }
    }
}
