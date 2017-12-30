﻿using System;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace SharpGEDParser.Model
{
    // Low-level line range reference
    public class LineSet
    {
        // TODO 20160629 line references are relative to sub-record, not file
        public int Beg { get; set; }
        public int End { get; set; }

        public int LineCount { get { return End - Beg + 1; } }
    }

    /// <summary>
    /// Attributes about an unknown tag - custom or not
    /// </summary>
    public class UnkRec : LineSet
    {
        public enum ErrorCode
        {
            Exception = 1,
            UntermIdent,
            MissIdent,
            MultChan,
            InvXref, // 5
            MissName,
            ChanDate,
            InvNCHI,
            MultNCHI,
            MultId,  // 10
            MissFile,
            MissForm,
            RefSourText,
            EmbSourEven,
            MultCHIL, // 15
            MissHEAD,
            EmbSourPage,
            MultHUSB,
            MultWIFE,
            MultRESN, // 20
            InvRestrict,
            InvExtra,
            InvSex,
            EmptyLine,
            LineTooLong, // 25
            InvLevel,
            EmptyFile, // input file is empty
            MissTag, // missing tag
            IdentCollide, // more than one record with same ident
            UnsuppLB, // 30 // Unsupported line breaks (CR)
            MissCharSet, // No character set specified in header
            BOMMismatch, // BOM doesn't match specified charset
            ForceASCII, // unsupported or other problem charset; forced to ASCII
            NonStdAlias, // Non-standard ALIA tag
            CustTagsSeen, // 35 // one or more valid custom tags seen
            EmptyName, // NAME line with no value
            SlashInName, // Surname containing slash(es)
            UntermSurname, // Surname is not terminated with slash
        }

        public UnkRec()
        {
            Beg = End = -1;
        }

        public UnkRec(string tag, int beg, int end)
        {
            Tag = tag; // TODO null=="<unexpected>";
            Beg = beg;
            End = end;
        }

        public string Tag { get; set; }

        public ErrorCode Error { get; set; }
    }

    // Based on the data from https://www.genealogieonline.nl/en/GEDCOM-tags/ some properties are less
    // frequent and their instances are allocated on an as-needed basis.

    /// <summary>
    /// Properties which are common across the main top-level GEDCOM records.
    ///  
    /// I.e. INDI, FAM, OBJE, SOUR, NOTE, REPO records.
    /// </summary>
    public abstract class GEDCommon
    {
        /// The record's GEDCOM tag.
        public abstract string Tag { get; }

        /// The record's id
        public string Ident { get; set; }

        /// The first line of the record in the original GEDCOM
        public int BegLine { get; set; }

        /// The last line of the record in the original GEDCOM
        public int EndLine { get; set; }

        /// Any RIN
        public string RIN { get; set; }

        private ChangeRec _chan;
        /// Any change record (CHAN).
        public ChangeRec CHAN
        {
            get { return _chan ?? (_chan = new ChangeRec()); }
        }

        /// Unknown and custom tags encountered at this level
        private List<UnkRec> _unknowns;
        public List<UnkRec> Unknowns { get { return _unknowns ?? (_unknowns = new List<UnkRec>()); } }

        /// Problems, other than 'unknown'/'custom' tags at this _or_children_ level
        private List<UnkRec> _errors;
        public List<UnkRec> Errors { get { return _errors ?? (_errors = new List<UnkRec>()); } }

        /// Are there any errors associated with this record? Returns true if yes.
        public bool AnyErrors { get { return _errors != null && _errors.Count > 0; } }

        // The IdHold implementation proved to be memory expensive when used with files
        // containing large numbers of UIDs, AFNs. NOTE: GK uses a <tag,value> sort of
        // scheme for these and other tags, consider revisiting.

        /// An UID (universal identifier) associated with the record.
        public StringPlus UID { get; set; }
        /// An AFN (ancestral file number) associated with the record.
        public StringPlus AFN { get; set; }
        /// An RFN (something file number) associated with the record.
        public StringPlus RFN { get; set; }

        private List<StringPlus> _refns;
        /// Any user reference numbers associated with the record. Will be null if none.
        public List<StringPlus> REFNs { get { return _refns ?? (_refns = new List<StringPlus>()); } }
        /// Returns true if there are any REFNs associated to the record.
        public bool AnyREFNs { get { return _refns != null; } }

        // TODO revisit this, esp. not using StringPlus if not required for UID/AFN/RFN/REFN
        //// Container for other ids (REFN, UID, AFN, RFN)
        //// NOTE: RIN is not here because used by > 50% of records
        //private IdHold _ids;
        //public IdHold Ids { get { return _ids ?? (_ids = new IdHold()); } }

        internal GEDCommon(GedRecord lines, string ident)
        {
            Ident = ident;
            if (lines == null) // DrawAnce creating INDI on-the-fly
                return;
            BegLine = lines.Beg;
            EndLine = lines.End;
        }
    }

    public class ChangeRec : StructCommon, NoteHold
    {
        public DateTime? Date { get; set; }

        private List<Note> _notes;

        public List<Note> Notes { get { return _notes ?? (_notes = new List<Note>()); } }
    }

    public class IdHold
    {
        private List<StringPlus> _refns;
        public List<StringPlus> REFNs { get { return _refns ?? (_refns = new List<StringPlus>()); } }

        private Dictionary<string, StringPlus> _other;
        public Dictionary<string, StringPlus> Others { get { return _other ?? (_other = new Dictionary<string, StringPlus>()); } }

        public bool HasId(string tag)
        {
            return Others.ContainsKey(tag);
        }

        public void Add(string tag, StringPlus sp)
        {
            Others.Add(tag, sp);
        }
    }

    public class StringPlus
    {
        public string Value { get; set; }

        private LineSet _extra;
        public LineSet Extra { get { return _extra ?? (_extra = new LineSet()); } }
    }
}
