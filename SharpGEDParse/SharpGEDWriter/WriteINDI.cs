﻿using SharpGEDParser.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// TODO can record ident be generalized?

namespace SharpGEDWriter
{
    class WriteINDI
    {
        internal static void WriteINDIs(StreamWriter file, List<GEDCommon> records)
        {
            foreach (var gedCommon in records)
            {
                if (gedCommon is IndiRecord)
                    WriteOneIndi(file, gedCommon as IndiRecord);
            }
        }

        private static void writeLink(StreamWriter file, IndiLink indiLink)
        {
            // TODO extra text
            var str = indiLink.Type == IndiLink.FAMS_TYPE ? "FAMS" : "FAMC";
            WriteCommon.writeXrefIfNotEmpty(file, str, indiLink.Xref, 1);
            WriteCommon.writeIfNotEmpty(file, "PEDI", indiLink.Pedi, 2);
            WriteCommon.writeIfNotEmpty(file, "STAT", indiLink.Stat, 2);
            WriteCommon.writeSubNotes(file, indiLink, 2);
        }

        // TODO more closely match PAF order? - specifically _UID
        // INDI records are written to be as close to PAF order as possible
        private static void WriteOneIndi(StreamWriter file, IndiRecord indiRecord)
        {
            file.WriteLine("0 @{0}@ INDI", indiRecord.Ident);

            WriteCommon.writeIfNotEmpty(file, "RESN", indiRecord.Restriction, 1);

            foreach (var nameRec in indiRecord.Names)
            {
                writeName(file, nameRec);

                // this convolution below due to:
                // 1 NAME Joe /Blow/ Jr.
                // vs
                // 1 NAME Joe
                // 2 GIVN Joe
                // 2 SURN Blow
                // TODO nice to output parts in specific order despite how they came in

                bool didGivn = false;
                bool didSurn = false;
                bool didNSFX = false;

                foreach (var tuple in nameRec.Parts)
                {
                    file.WriteLine("2 {0} {1}", tuple.Item1, tuple.Item2);
                    if (tuple.Item1 == "SURN")
                        didSurn = true;
                    if (tuple.Item1 == "GIVN")
                        didGivn = true;
                    if (tuple.Item1 == "NSFX")
                        didNSFX = true;
                }
                if (!didGivn && !string.IsNullOrEmpty(nameRec.Names))
                {
                    file.WriteLine("2 GIVN {0}", nameRec.Names);
                }
                if (!didSurn && !string.IsNullOrEmpty(nameRec.Surname))
                {
                    file.WriteLine("2 SURN {0}", nameRec.Surname);
                }
                if (!didNSFX && !string.IsNullOrEmpty(nameRec.Suffix))
                {
                    file.WriteLine("2 NSFX {0}", nameRec.Suffix);
                }
                // TODO other name pieces

                WriteCommon.writeSubNotes(file, nameRec, 2);
                WriteCommon.writeSourCit(file, nameRec, 2);
            }

            // TODO want to init sex to 'U' but don't want to output it if not initialized as such
            if (string.IsNullOrEmpty(indiRecord.FullSex) && indiRecord.Sex != '\0' && indiRecord.Sex != 'U')
                WriteCommon.writeIfNotEmpty(file, "SEX", indiRecord.Sex.ToString(), 1);
            else
                WriteCommon.writeIfNotEmpty(file, "SEX", indiRecord.FullSex, 1);

            WriteEvent.writeEvents(file, indiRecord.Events, 1);
            WriteEvent.writeEvents(file, indiRecord.Attribs, 1);

            // Insure FAMS/FAMC output in consistent order
            if (indiRecord.Links != null)
            {
                foreach (var indiLink in indiRecord.Links.Where(indiLink => indiLink.Type == IndiLink.FAMS_TYPE))
                {
                    writeLink(file, indiLink);
                }
                foreach (var indiLink in indiRecord.Links.Where(indiLink => indiLink.Type == IndiLink.FAMC_TYPE))
                {
                    writeLink(file, indiLink);
                }
            }

            // TODO LDS events

            foreach (var aliasLink in indiRecord.AliasLinks)
            {
                WriteCommon.writeXrefIfNotEmpty(file, "ALIA", aliasLink, 1);
            }

            foreach (var assoRec in indiRecord.Assocs)
            {
                WriteCommon.writeXrefIfNotEmpty(file, "ASSO", assoRec.Ident, 1);
                WriteCommon.writeIfNotEmpty(file, "RELA", assoRec.Relation, 2);
                WriteCommon.writeSubNotes(file, assoRec, 2);
                WriteCommon.writeSourCit(file, assoRec, 2);
            }

            // Different from FAM because there are different types
            foreach (var submitter in indiRecord.Submitters)
            {
                string tag = "";
                switch (submitter.SubmitterType)
                {
                    case Submitter.SubmitType.SUBM:
                        tag = "SUBM";
                        break;
                    case Submitter.SubmitType.ANCI:
                        tag = "ANCI";
                        break;
                    case Submitter.SubmitType.DESI:
                        tag = "DESI";
                        break;
                }
                file.WriteLine("1 {0} @{1}@", tag, submitter.Xref);
            }

            WriteCommon.writeRecordTrailer(file, indiRecord, 1);
        }

        private static void writeName(StreamWriter file, NameRec name)
        {
            var names = "";
            if (!string.IsNullOrWhiteSpace(name.Names))
                names = name.Names;
            var sur = "";
            if (!string.IsNullOrWhiteSpace(name.Surname))
                sur = "/" + name.Surname + "/";
            var suf = "";
            if (!string.IsNullOrWhiteSpace(name.Suffix))
                suf = name.Suffix;
            string line = string.Format("1 NAME {0}{1}{2}{3}{4}", names, 
                names.Length > 1 ? " " : "",
                sur,
                suf.Length > 1 ? " " : "",
                suf
                );
            file.WriteLine(line.Trim());
        }
    }
}
