using System;
using System.Collections.Generic;

namespace InazumaElevenGoMapEventEditor.InazumaElevenGo.Logic
{
    public class NPC
    {
        public string Name;

        public NPCBase Base;

        public List<NPCAppear> Appears;

        public List<Event> Events;

        public NPC()
        {

        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class NPCBase
    {
        public int ID;
        public int HeadID;
        public int Type;
        public int Unknown1;
        public int UniformID;
        public int BootID;
        public int GloveID;
        public int IconID;

        public NPCBase(int[] values)
        {
            int valueIndex = 0;
            var fields = typeof(NPCBase).GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var fieldType = field.FieldType;

                if (fieldType.IsArray)
                {
                    int arrayLength = ((Array)field.GetValue(this)).Length;
                    Array.Copy(values, valueIndex, (Array)field.GetValue(this), 0, arrayLength);
                    valueIndex += arrayLength;
                }
                else
                {
                    field.SetValue(this, values[valueIndex]);
                    valueIndex++;
                }
            }
        }

        public int[] ToArray()
        {
            List<int> values = new List<int>();
            var fields = typeof(NPCBase).GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var fieldType = field.FieldType;

                if (fieldType.IsArray)
                {
                    values.AddRange((int[])field.GetValue(this));
                }
                else
                {
                    values.Add((int)field.GetValue(this));
                }
            }

            return values.ToArray();
        }
    }

    public class NPCPreset
    {
        public int ID;
        public int NPCAppearStartOffset;
        public int NPCAppearCount;

        public NPCPreset(int[] values)
        {
            int valueIndex = 0;
            var fields = typeof(NPCPreset).GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var fieldType = field.FieldType;

                if (fieldType.IsArray)
                {
                    int arrayLength = ((Array)field.GetValue(this)).Length;
                    Array.Copy(values, valueIndex, (Array)field.GetValue(this), 0, arrayLength);
                    valueIndex += arrayLength;
                }
                else
                {
                    field.SetValue(this, values[valueIndex]);
                    valueIndex++;
                }
            }
        }

        public int[] ToArray()
        {
            List<int> values = new List<int>();
            var fields = typeof(NPCPreset).GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var fieldType = field.FieldType;

                if (fieldType.IsArray)
                {
                    values.AddRange((int[])field.GetValue(this));
                }
                else
                {
                    values.Add((int)field.GetValue(this));
                }
            }

            return values.ToArray();
        }
    }

    public class NPCAppear
    {
        public int LocationX;
        public int Unk1;
        public int LocationY;
        public int Unk3;
        public int Unk4;
        public int Unk5;
        public int Unk6;
        public int Unk7;
        public int Unk8;
        public int Unk9;
        public int Unk10;
        public int PhaseAppear;
        public int Unk12;

        public NPCAppear(int[] values)
        {
            int valueIndex = 0;
            var fields = typeof(NPCAppear).GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var fieldType = field.FieldType;

                if (fieldType.IsArray)
                {
                    int arrayLength = ((Array)field.GetValue(this)).Length;
                    Array.Copy(values, valueIndex, (Array)field.GetValue(this), 0, arrayLength);
                    valueIndex += arrayLength;
                }
                else
                {
                    field.SetValue(this, values[valueIndex]);
                    valueIndex++;
                }
            }
        }

        public int[] ToArray()
        {
            List<int> values = new List<int>();
            var fields = typeof(NPCAppear).GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var fieldType = field.FieldType;

                if (fieldType.IsArray)
                {
                    values.AddRange((int[])field.GetValue(this));
                }
                else
                {
                    values.Add((int)field.GetValue(this));
                }
            }

            return values.ToArray();
        }
    }
}
