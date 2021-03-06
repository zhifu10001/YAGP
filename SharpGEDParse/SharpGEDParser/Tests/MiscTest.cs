﻿using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using SharpGEDParser.Model;

// ReSharper disable UnusedMethodReturnValue.Local
// ReSharper disable ConvertToConstant.Local
// ReSharper disable InconsistentNaming

namespace SharpGEDParser.Tests
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    class MiscTest : GedParseTest
    {
        [Test]
        public void TestMethod1()
        {
            string testString = "0 HEAD\n1 SOUR TJ\n2 NAME Tamura Jones\n2 VERS 1.0\n1 DATE 9 Sep 2013\n1 FILE IdentCONT.GED\n1 NOTE Test File: CONT line with identifier.\n1 GEDC\n2 VERS 5.5.1\n2 FORM LINEAGE-LINKED\n1 CHAR UTF-8\n1 LANG English\n1 SUBM @U1@\n0 @U1@ SUBM\n1 NAME Name\n0 @I1@ INDI\n1 NAME One /Note/\n2 SURN Note\n2 GIVN One\n1 NOTE First line of a note.\n2 @IDENT@ CONT Second line of a note.\n2 CONT Third line of a note.\n0 TRLR";
            var res = ReadIt(testString);

            Assert.AreEqual(3, res.Count);
            Assert.IsNotNull(res[2] as IndiRecord);
        }

        [Test]
        public void TestMethod2()
        {
            string testString =
                "0 HEAD\n1 CHAR ASCII\n1 SOUR ID_OF_CREATING_FILE\n1 GEDC\n2 VERS 5.5\n2 FORM Lineage-Linked\n1 SUBM @SUBMITTER@\n" +
                "0 @SUBMITTER@ SUBM\n1 NAME /Submitter/\n1 ADDR Submitters address\n2 CONT address continued here\n0 @FATHER@ INDI\n" +
                "1 NAME /Father/\n1 SEX M\n1 BIRT\n2 PLAC birth place\n2 DATE 1 JAN 1899\n1 DEAT\n2 PLAC death place\n2 DATE 31 DEC 1990\n" +
                "1 FAMS @FAMILY@\n0 @MOTHER@ INDI\n1 NAME /Mother/\n1 SEX F\n1 BIRT\n2 PLAC birth place\n2 DATE 1 JAN 1899\n1 DEAT\n" +
                "2 PLAC death place\n2 DATE 31 DEC 1990\n1 FAMS @FAMILY@\n0 @CHILD@ INDI\n1 NAME /Child/\n1 BIRT\n2 PLAC birth place\n" +
                "2 DATE 31 JUL 1950\n1 DEAT\n2 PLAC death place\n2 DATE 29 FEB 2000\n1 FAMC @FAMILY@\n0 @FAMILY@ FAM\n1 MARR\n" +
                "2 PLAC marriage place\n2 DATE 1 APR 1950\n1 HUSB @FATHER@\n1 WIFE @MOTHER@\n1 CHIL @CHILD@\n0 TRLR";
            var fr = ReadItHigher(testString);
            var res = fr.Data;

            Assert.AreEqual(6, res.Count);
            // TODO GedCommon doesn't expose Tag property
            Assert.IsNotNull(res[2] as IndiRecord);
            Assert.IsNotNull(res[3] as IndiRecord);
            Assert.IsNotNull(res[4] as IndiRecord);
            Assert.IsNotNull(res[5] as FamRecord);
        }

        //[Test]
        //public void IllegalAdopLevel()
        //{
        //    // TODO not being caught because 20161125 not yet parsing PLAC sub-record
        //    // ADOP as a sub-record of PLAC is an error
        //    string indi = "0 INDI\n1 BIRT Y\n2 FAMC @FAM99@\n2 PLAC Sands, Oldham, Lncshr, Eng\n3 ADOP pater";
        //    var rec = parse<IndiRecord>(indi, "INDI");
        //    Assert.AreEqual(1, rec.Events.Count);
        //    Assert.AreNotEqual(0, rec.Events[0].Errors.Count); 
        //    Assert.AreNotEqual("pater", rec.Events[0].FamcAdop); // TODO currently broken
        //}

        // TODO ADOP as a sub-record of AGNC, RELI, RESN, CAUS, TYPE, DATE, AGE would be error

        [Test]
        public void BogusText()
        {
            // NOTE the third line is missing the leading level #
            string indi = "0 INDI\n1 DSCR attrib_value\nCONC a big man\n2 CONT I don't know the\n2 CONT secret handshake\n2 DATE 1774\n2 PLAC Sands, Oldham, Lncshr, Eng\n2 AGE 17\n2 TYPE suspicious";
            var fr = ReadItHigher(indi);
            Assert.AreEqual(1, fr.Data.Count);
            var rec = fr.Data[0] as IndiRecord;
            Assert.IsNotNull(rec);
            Assert.AreEqual(1, rec.Attribs.Count, "Attribute not parsed");
            //Assert.AreEqual(1, rec.Attribs[0].Errors.Count, "Error not recorded in attribute");
        }

        [Test]
        public void BogusText2()
        {
            // NOTE the third line has an invalid leading level # (value below '0')
            // TODO which is changing how the upper level record is accumulated
            string indi = "0 INDI\n1 BIRT attrib_value\n+ CONC a big man\n2 CONT I don't know the\n2 CONT secret handshake\n2 DATE 1774\n2 PLAC Sands, Oldham, Lncshr, Eng\n2 AGE 17\n2 TYPE suspicious";
            var rec = parse<IndiRecord>(indi);
            Assert.AreEqual(1, rec.Events.Count, "Event not parsed");
            // TODO no place to store errors
            //Assert.AreNotEqual(0, rec.Events[0].Errors.Count, "Error not recorded"); // TODO verify details
        }

        [Test]
        public void BogusText3()
        {
            // NOTE the third line has an invalid leading level # (value above '9')
            string indi = "0 INDI\n1 BIRT attrib_value\nA CONC a big man\n2 CONT I don't know the\n2 CONT secret handshake\n2 DATE 1774\n2 PLAC Sands, Oldham, Lncshr, Eng\n2 AGE 17\n2 TYPE suspicious";
            var rec = parse<IndiRecord>(indi);
            Assert.AreEqual(1, rec.Events.Count, "Event not parsed");
            // TODO no place to store errors
            //Assert.AreNotEqual(0, rec.Events[0].Errors.Count, "Error not recorded");
        }

        [Test]
        public void NotZero()
        {
            // No zero level at first line - can't parse
            var indi = "INDI\n1 DSCR attrib_value\nCONC a big man\n2 CONT I don't know the\n2 CONT secret handshake\n2 DATE 1774\n2 PLAC Sands, Oldham, Lncshr, Eng\n2 AGE 17\n2 TYPE suspicious";
            var fr = ReadItHigher(indi);
            Assert.AreEqual(1, fr.Errors.Count);
            Assert.AreEqual(UnkRec.ErrorCode.InvLevel, fr.Errors[0].Error);
            Assert.AreEqual(0, fr.Data.Count);
        }

        [Test]
        public void EmptyLines()
        {
            // multiple empty lines (incl. blanks)
            string indi = "0 INDI\n1 DSCR attrib_value\n\n2 DATE 1774\n   ";
            var fr = ReadItHigher(indi);
            Assert.AreEqual(1, fr.Errors.Count); // blank lines as error "records"
            Assert.AreEqual(UnkRec.ErrorCode.EmptyLine, fr.Errors[0].Error);
            Assert.AreEqual(1, fr.AllIndividuals.Count);
            Assert.AreEqual(1, fr.AllIndividuals[0].Errors.Count);
            Assert.AreEqual(UnkRec.ErrorCode.MissIdent, fr.AllIndividuals[0].Errors[0].Error);
        }

        [Test]
        public void LeadingSpaces()
        {
            string indi = "0 INDI\n     1 DSCR attrib_value\n     2 DATE 1774";
            string indi2 = "     0 INDI\n     1 DSCR attrib_value\n     2 DATE 1774";
            var rec = parse<IndiRecord>(indi);
            Assert.AreEqual(1, rec.Attribs.Count);
            rec = parse<IndiRecord>(indi2);
            Assert.AreEqual(1, rec.Attribs.Count);
        }

        [Test]
        public void LeadingSpaces2()
        {
            string indi = "0 INDI\n     1 NAME \t \tattrib_value\n     2 DATE 1774";
            var rec = parse<IndiRecord>(indi);
            Assert.AreEqual(1, rec.Names.Count);
        }

        [Test]
        public void LeadingTabs()
        {
            string indi = "0 INDI\n\t\t1 DSCR attrib_value\n\t\t2 DATE 1774";
            var rec = parse<IndiRecord>(indi);
            Assert.AreEqual(1, rec.Attribs.Count);
        }

        [Test]
        public void Malform()
        {
            // exercise infinite loop found when no '1' level line
            string indi = "0 INDI\n2 DATE 1774";
            var rec = parse<IndiRecord>(indi);
            Assert.AreEqual(0, rec.Attribs.Count);
            // TODO error? - INDI record with no data
        }

        [Test,Ignore("Error lines not saved on MissTag")]
        public void NoTagSub()
        {
            // allefamilierelationer.ged had a line with no tag "1  ". Error was flagged on wrong line and stopped parsing.
            var indi = "0 @I1@ INDI\n1 DSCR attrib_value\n2 CONC a big man\n2 CONT I don't know the\n2 CONT secret handshake\n2  \n2 DATE 1774\n2 PLAC Sands, Oldham, Lncshr, Eng\n2 AGE 17\n2 TYPE suspicious\n1 NAME /Father/\n1 SEX M";
            var rec = parse<IndiRecord>(indi);

            Assert.AreEqual(0, rec.Errors.Count);

            Assert.AreEqual("I1", rec.Ident);
            Assert.AreEqual('M', rec.Sex);

            // The 'other line' here is on the sub-structure
            Assert.AreEqual(1, rec.Attribs.Count);
            Assert.AreEqual(1, rec.Errors.Count);
            Assert.AreEqual(UnkRec.ErrorCode.MissTag, rec.Errors[0].Error);
            // TODO the MissTag handling prevents saving the error line in OtherLines
            Assert.AreEqual(1, rec.Attribs[0].OtherLines.Count);
            Assert.AreEqual(5, rec.Attribs[0].OtherLines[0].Beg);
        }

        [Test]
        public void NoTagSub2()
        {
            // allefamilierelationer.ged had a line with no tag "1  ". Error was flagged on wrong line and stopped parsing.
            var indi = "0 @I1@ INDI\n1 DSCR attrib_value\n2 CONC a big man\n2 CONT I don't know the\n2 CONT secret handshake\n2 DATE 1774\n2 PLAC Sands, Oldham, Lncshr, Eng\n2 AGE 17\n2 TYPE suspicious\n1  \n1 NAME /Father/\n1 SEX M";
            var rec = parse<IndiRecord>(indi);

            Assert.AreEqual(0, rec.Errors.Count);

            Assert.AreEqual("I1", rec.Ident);
            Assert.AreEqual('M', rec.Sex);

            Assert.AreEqual(1, rec.Attribs.Count);
            Assert.AreEqual(0, rec.Errors.Count);
            Assert.AreEqual(0, rec.Attribs[0].OtherLines.Count);

            // The 'unknown' line has been recorded in the parent record
            Assert.AreEqual(1, rec.Unknowns.Count);
            Assert.AreEqual(10, rec.Unknowns[0].Beg);
            Assert.IsNull(rec.Unknowns[0].Tag);
        }

        //[Test]
        //public void NoTagProb()
        //{
        //    // allefamilierelationer.ged had a line with no tag "1  ". Error was flagged on wrong line and stopped parsing.
        //    var indi = "0 @I1@ INDI\n1 DSCR attrib_value\n2 CONC a big man\n2 CONT I don't know the\n2 CONT secret handshake\n2 DATE 1774\n2 PLAC Sands, Oldham, Lncshr, Eng\n2 AGE 17\n2 TYPE suspicious\n1 SEX M\n1  \n1 NAME /Father/";
        //    var rec = parse<IndiRecord>(indi, "INDI");

        //    // TODO The bogus line is being treated as a 'SEX' line???
        //    Assert.AreEqual('M', rec.Sex);

        //    Assert.AreEqual(1, rec.Errors.Count);

        //    Assert.AreEqual("I1", rec.Ident);

        //    // The error has been recorded on the sub-structure
        //    Assert.AreEqual(1, rec.Attribs.Count);
        //    Assert.AreEqual(1, rec.Attribs[0].Errors.Count);
        //    Assert.AreEqual(9, rec.Attribs[0].Errors[0].Beg);
        //    Assert.IsNotNull(rec.Attribs[0].Errors[0].Error);
        //}

        [Test]
        public void LineTooLong()
        {
            // TODO how to exercise for UTF-16 ?
            string indi = "0 INDI\n1 DSCR attrib_value" +
            "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
            "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
            "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                          "\n2 DATE 1774";
            var fr = ReadItHigher(indi);
            Assert.AreEqual(1, fr.Data.Count);
            Assert.AreEqual(1, fr.Errors.Count); // long lines as error "records"
        }

        [Test]
        public void runOnConc()
        {
            // a later CONC tag was picked up by a tag which took a CONC
            var txt = "0 INDI\n1 NOTE notes \n2 CONC detail\n1 DSCR a big man\n2 CONT I don't";
            var rec = parse<IndiRecord>(txt);
            Assert.AreEqual("notes detail", rec.Notes[0].Text); // i.e. do NOT pick up the CONT from DSCR
        }

        [Test]
        public void LeadingSpacesCONT()
        {
            // CONC / CONT tags with leading spaces
            var indi = "0 INDI\n1 NOTE notes\n    2 CONT more detail";
            var rec = parse<IndiRecord>(indi);
            Assert.AreEqual(1, rec.Notes.Count);
            Assert.AreEqual("notes\nmore detail", rec.Notes[0].Text);
        }

        [Test]
        public void LeadingSpacesCONC()
        {
            // CONC / CONT tags with leading spaces
            var indi = "0 INDI\n1 NOTE notes \n    2 CONC more detail";
            var rec = parse<IndiRecord>(indi);
            Assert.AreEqual(1, rec.Notes.Count);
            Assert.AreEqual("notes more detail", rec.Notes[0].Text);
        }

        // TODO where else might leading spaces be a problem? SOUR/DATA/DATE? SOUR/DATA/TEXT? SOUR/TEXT?

        [Test]
        public void EmptyCONT()
        {
            // one GEDCOM had CONT lines with no text
            var txt = "0 INDI\n1 NOTE blah\n2 CONT";
            var rec = parse<IndiRecord>(txt);
            Assert.AreEqual(1, rec.Notes.Count);
            Assert.AreEqual("blah\n", rec.Notes[0].Text);
        }

        [Test]
        public void Unk()
        {
            var txt = "0 BLAH\n";
            var res = ReadIt(txt);

            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("BLAH", res[0].Tag);
        }

        [Test]
        public void AllFam()
        {
            string indi = "0 @F1@ FAM\n1 HUSB @I1@\n1 WIFE @I2@";
            var fr = ReadItHigher(indi);
            Assert.AreEqual(0, fr.Errors.Count);
            Assert.AreEqual(1, fr.AllFamilies.Count);
            Assert.AreEqual(0, fr.AllFamilies[0].Errors.Count);
        }

        [Test]
        public void AllSour()
        {
            string indi = "0 @F1@ FAM\n1 HUSB @I1@\n1 WIFE @I2@\n0 @S1@ SOUR";
            var fr = ReadItHigher(indi);
            Assert.AreEqual(0, fr.Errors.Count);
            Assert.AreEqual(1, fr.AllFamilies.Count);
            Assert.AreEqual(1, fr.AllSources.Count);
        }

        [Test]
        public void InvalidLevel()
        {
            // code coverage
            string txt = "1 CHAN\n";
            var fr = ReadItHigher(txt);
            Assert.AreEqual(1, fr.Data.Count);
            var rec = fr.Data[0];
            Assert.IsTrue(rec is Unknown);
            Assert.AreEqual(0, rec.Errors.Count);
        }

        [Test]
        public void EmptyTag()
        {
            // code coverage
            string txt = "0 @Z1@ \n";
            var fr = ReadItHigher(txt);
            Assert.AreEqual(1, fr.AllErrors.Count);
            Assert.AreEqual(1, fr.Data.Count);
            var rec = fr.Data[0];
            Assert.IsTrue(rec is Unknown);
            Assert.AreEqual(UnkRec.ErrorCode.MissTag, rec.Errors[0].Error);
        }

        [Test]
        public void OthersFileLineNums()
        {
            // Verify that 'other lines' in a sub-structure are correctly relative to the 'file'
            var txt = "0 @I1@ INDI\n1 NAME /Blah/\n0 @I2@ INDI\n1 CHAN\n2 DATE 6 JAN 2018\n2 CUSTOM\n1 NAME /Blah/";
            var res = ReadIt(txt);
            Assert.AreEqual(2, res.Count);
            var rec = res[1];
            Assert.IsNotNull(rec.CHAN);
            Assert.AreEqual(1, rec.CHAN.OtherLines.Count);
            Assert.AreEqual(6, rec.CHAN.OtherLines[0].Beg); // NOTE: relies on read-from-stream starting at line '1', with no '0 HEAD'
        }

        [Test]
        public void SourCitText()
        {
            // During GedValid development, INDI.SOUR.TEXT error did not have correct end line
            var txt = "0 @I1@ INDI\n1 NAME /Blah/\n1 SOUR @S1@\n2 TEXT Blah\n2 EVEN event\n1 BIRT\n2 DATE 21 Feb 1696\n2 PLAC North Hampton, Rockingham County, New Hampshire";

            var res = ReadIt(txt);
            Assert.AreEqual(1, res.Count);
            IndiRecord rec = res[0] as IndiRecord;
            Assert.AreEqual(1, rec.Events.Count);
            Assert.AreEqual(1, rec.Errors.Count);
            Assert.AreEqual(UnkRec.ErrorCode.RefSourText, rec.Errors[0].Error);
            Assert.AreEqual(3, rec.Errors[0].Beg);
            Assert.AreEqual(5, rec.Errors[0].End);
        }

        [Test]
        public void SourEmbSourEven()
        {
            // During GedValid development, INDI.SOUR.TEXT error did not have correct end line
            var txt = "0 @I1@ INDI\n1 NAME /Blah/\n1 SOUR description\n2 TEXT Blah\n2 EVEN event\n1 BIRT\n2 DATE 21 Feb 1696\n2 PLAC North Hampton, Rockingham County, New Hampshire";

            var res = ReadIt(txt);
            Assert.AreEqual(1, res.Count);
            IndiRecord rec = res[0] as IndiRecord;
            Assert.AreEqual(1, rec.Events.Count);
            Assert.AreEqual(1, rec.Errors.Count);
            Assert.AreEqual(UnkRec.ErrorCode.EmbSourEven, rec.Errors[0].Error);
            Assert.AreEqual(3, rec.Errors[0].Beg);
            Assert.AreEqual(5, rec.Errors[0].End);
        }

        [Test]
        public void SourInvXref()
        {
            // During GedValid development, INDI.SOUR.TEXT error did not have correct end line
            var txt = "0 @I1@ INDI\n1 NAME /Blah/\n1 SOUR @ @\n1 BIRT\n2 DATE 21 Feb 1696\n2 PLAC North Hampton, Rockingham County, New Hampshire";

            var res = ReadIt(txt);
            Assert.AreEqual(1, res.Count);
            IndiRecord rec = res[0] as IndiRecord;
            Assert.AreEqual(1, rec.Events.Count);
            Assert.AreEqual(1, rec.Errors.Count);
            Assert.AreEqual(UnkRec.ErrorCode.InvXref, rec.Errors[0].Error);
            Assert.AreEqual(3, rec.Errors[0].Beg);
            Assert.AreEqual(3, rec.Errors[0].End);
        }

        [Test]
        public void SourEmbSourPage()
        {
            // During GedValid development, INDI.SOUR.TEXT error did not have correct end line
            var txt = "0 @I1@ INDI\n1 NAME /Blah/\n1 SOUR description\n2 TEXT Blah\n2 PAGE event\n1 BIRT\n2 DATE 21 Feb 1696\n2 PLAC North Hampton, Rockingham County, New Hampshire";

            var res = ReadIt(txt);
            Assert.AreEqual(1, res.Count);
            IndiRecord rec = res[0] as IndiRecord;
            Assert.AreEqual(1, rec.Events.Count);
            Assert.AreEqual(1, rec.Errors.Count);
            Assert.AreEqual(UnkRec.ErrorCode.EmbSourPage, rec.Errors[0].Error);
            Assert.AreEqual(3, rec.Errors[0].Beg);
            Assert.AreEqual(5, rec.Errors[0].End);
        }

        [Test]
        public void SourCitTextOwner()
        {
            // Errors were only going to 'owning' structure. E.g. errors were tracked in NameRec, not the Indi.

            // The following errors exist: (RefSourText being a TEXT tag used with a reference SOUR)
            // 1. RefSourText for the NAME
            // 2. RefSourText for the INDI
            // 3. RefSourText for BIRT
            // 4. RefSourText for BIRT
            // 5. RefSourText for BIRT
            var txt =
                "0 @I1246@ INDI\n1 NAME Mehitable /DEARBORN/\n2 SOUR @S36@\n3 TEXT Mehitable Dearborn\n1 SEX F\n1 SOUR @S37@\n2 TEXT Mehetabel Dearborn" +
                "\n1 BIRT\n2 DATE 21 Feb 1696/97\n2 PLAC North Hampton, Rockingham County, New Hampshire\n2 SOUR @S37@\n3 TEXT 21 Feb 1696/97 (Rauch Files, Adam Rauch, manuscript)" +
                "\n2 SOUR @S36@\n3 TEXT 21 Feb 1696/97\n2 SOUR @S44@\n3 TEXT 21 Feb 1696/97, North Hampton, Rockingham County, New Hampshire\n1 DEAT\n2 DATE 1 AUG 1758" +
                "2 SOUR @S44@\n1 FAMC @F565@\n1 FAMS @F964@\n1 CHAN\n2 DATE 28 MAR 2001";
            var res = ReadIt(txt);
            Assert.AreEqual(1, res.Count);
            IndiRecord rec = res[0] as IndiRecord;
            Assert.AreEqual(5, rec.Errors.Count); // all errors owned by parent
        }
    }
}

// TODO custom tags
// TODO unknown tags

// TODO an integer level value > 9, e.g. "10 PLAC someplace"
// TODO leading spaces and an integer level value > 9, e.g. "   10 CONT continued"
// TODO an ident within a CONC/CONT tag, e.g. "2 @3@ CONT continued"
// TODO invalid level value NOT in sub-record, e.g. "0 INDI\nA bogus "???

// TODO xref parsing
// From Tamura Jones:
/*
Issue an error for non-ASCII characters in a GEDCOM identifier, and abort.
Upon encountering a C0 Control Character (0x00-0x1F) in a GEDCOM identifier, report a fatal error and abort.
A Horizontal Tab is invisible, assist the user by mentioning that control character by name.
Upon encountering a colon in a GEDCOM identifier, report a fatal error and abort.
Upon encountering an exclamation mark in a GEDCOM identifier, report a fatal error and abort.
Upon encountering an underscore in a GEDCOM identifier, report a fatal error and abort.
Upon encountering a number sign in a GEDCOM identifier; simply continue.
Upon encountering a space in a GEDCOM identifier, report a warning and continue.
For any other non-alphanumeric character in a GEDCOM identifier: simply continue.
*/