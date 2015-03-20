﻿using System;
using System.Collections.Generic;

namespace SharpGEDParser
{
    public class Rec
    {
        public int Beg { get; set; }

        public int End { get; set; }

        public string Tag { get; set; } // TODO enum?

        public override string ToString()
        {
            return string.Format("{0}({1},{2})", Tag, Beg, End);
        }
    }

    public class UnkRec : Rec
    {
        public UnkRec(string tag)
        {
            Tag = tag;
        }
    }

    public class SourceRec : XRefRec
    {
        public SourceRec(string xref) : base ("SOUR", xref)
        {
        }
    }

    // Currently used for _UID
    public class DataRec : Rec
    {
        public string Data { get; set; }

        public DataRec(string tag, string data)
        {
            Tag = tag;
            Data = data;
        }
    }

    public class ChildLinkRec : XRefRec
    {
        public ChildLinkRec(string ident) : base("FAMC", ident)
        {
        }
    }

    public class FamLinkRec : XRefRec
    {
        public FamLinkRec(string ident) : base("FAMS", ident)
        {
        }
    }

    public class XRefRec : Rec
    {
        public string XRef { get; set; }
        public XRefRec(string tag, string xref)
        {
            Tag = tag;
            XRef = xref;
        }
        public override string ToString()
        {
            return string.Format("{0}[{3}]({1},{2})", Tag, Beg, End, XRef);
        }
    }

    // NOTE: also used for birth (FAMC extra); ADOP (FAMC, ADOP extra)
    public class EventRec : Rec
    {
        public string Date { get; set; }
        public string Place { get; set; }

        public string Age { get; set; }

        public Tuple<int, int> Change { get; set; }

        public Tuple<int, int> Note { get; set; }

        public Tuple<int, int> Source { get; set; }

        public EventRec(string tag)
        {
            Tag = tag;
        }

        public override string ToString()
        {
            string note = Note != null ? ",Note:" + Note : "";
            string chan = Change != null ? ",Chan:" + Change : "";
            string src = Source != null ? ",Sour:" + Source : "";
            return string.Format("__{0}:When:'{1}',Where:'{2}'{3}{4}{5}", 
                base.ToString(), Date, Place, note, chan, src);
        }
    }

    public class NameRec : Rec
    {
        public string Names { get; set; }
        public string Surname { get; set; }
        public override string ToString()
        {
            return string.Format(" {0} /{1}/", Names, Surname);
        }
    }

    public class KBRGedIndi : KBRGedRec
    {
        // Restriction notice
        public string Restriction { get; set; }

        // Individual's sex
        public char Sex { get; set; }

        // Permanent record information
        public string PermanentRecord { get; set; }

        // Source records
        public List<SourceRec> Sources { get; set; }

        // Unknown records
        public List<UnkRec> Unknowns { get; set; }

        public List<EventRec> Events { get; set; }

        public List<EventRec> FamEvents { get; set; }

        public List<NameRec> Names { get; set; }

        public List<XRefRec> Alia { get; set; }
        public List<XRefRec> Anci { get; set; }
        public List<XRefRec> Desi { get; set; }
        public List<XRefRec> Subm { get; set; }

        public List<ChildLinkRec> ChildLinks { get; set; }

        public List<FamLinkRec> FamLinks { get; set; }

        public List<DataRec> Data { get; set; }

        public bool Living { get; set; }

        public Tuple<int, int> Note { get; set; }
        public Tuple<int, int> Change { get; set; }

        public KBRGedIndi(GedRecord lines, string ident) : base(lines)
        {
            Ident = ident;
            Tag = "INDI"; // TODO use enum?

            Sources = new List<SourceRec>();
            Unknowns = new List<UnkRec>();
            Events = new List<EventRec>();
            FamEvents = new List<EventRec>();
            Names = new List<NameRec>();
            Data = new List<DataRec>();

            Alia = new List<XRefRec>();
            Anci = new List<XRefRec>();
            Desi = new List<XRefRec>();
            Subm = new List<XRefRec>();

            ChildLinks = new List<ChildLinkRec>();
            FamLinks = new List<FamLinkRec>();

            Living = false;

            // TODO can any properties, especially List<>, not be initialized until use? Efficiently?
        }

        public override string ToString()
        {
            return string.Format("{0}({1}):{2}", Tag, Ident, Lines);
        }

    }
}