﻿using SharpGEDParser.Model;
using System.Collections.Generic;

// TODO validation should include checks for non-standard months, keywords

namespace SharpGEDParser.Parser
{
    static class EventDateParse
    {
        private enum Cal
        {
            Greg, Jul, Hebr, Fre, Rom, Fut
        }

        private enum KeyW // Used to determine how to convert to JulianDayRange
        {
            None, Before, After, From, InitialTo, Between, DontCare
        }

        private enum Era
        {
            BC, AD, None
        }

        private static Dictionary<string, Cal> calLookup = new Dictionary<string, Cal>
        {
            {"#DGREGORIAN", Cal.Greg},
            {"#DJULIAN", Cal.Jul},
            {"#DHEBREW", Cal.Hebr},
            {"#DFRENCH R", Cal.Fre},
            {"#DROMAN", Cal.Rom},
            {"#DUNKNOWN", Cal.Fut},
        };

        private static Dictionary<string, int> gregMonLookup = new Dictionary<string, int>()
        {
            {"JAN", 1},
            {"FEB", 2},
            {"MAR", 3},
            {"APR", 4},
            {"MAY", 5},
            {"JUN", 6},
            {"JUL", 7},
            {"AUG", 8},
            {"SEP", 9},
            {"OCT",10},
            {"NOV",11},
            {"DEC",12},
        };
        // Variations on the gregorian months - accepted but should be flagged
        // as non-standard
        private static Dictionary<string, int> gregMonNonStdLookup = new Dictionary<string, int>()
        {
            {"JAN.", 1},
            {"FEB.", 2},
            {"MAR.", 3},
            {"APR.", 4},
            {"MAY.", 5},
            {"JUN.", 6},
            {"JUL.", 7},
            {"AUG.", 8},
            {"SEP.", 9},
            {"OCT.",10},
            {"NOV.",11},
            {"DEC.",12},
            {"SEPT.", 9},
            {"SEPT", 9},
            {"JANUARY", 1},
            {"FEBRUARY", 2},
            {"MARCH", 3},
            {"APRIL", 4},
            {"JUNE", 6},
            {"JULY", 7},
            {"AUGUST", 8},
            {"SEPTEMBER", 9},
            {"OCTOBER",10},
            {"NOVEMBER",11},
            {"DECEMBER",12},
        };

        // The GEDCOM standard keywords - initial appearance
        private static Dictionary<string, KeyW> initialKeyLookup = new Dictionary<string, KeyW>()
        {
            {"BEF", KeyW.Before},
            {"AFT", KeyW.After},
            {"ABT", KeyW.DontCare},
            {"EST", KeyW.DontCare},
            {"CAL", KeyW.DontCare},
            {"FROM", KeyW.From},
            {"TO", KeyW.InitialTo},
            {"INT", KeyW.DontCare},
            {"BET", KeyW.Between}
        };
        // TODO 'NOT' variation: "NOT BEF"==after, "NOT AFT"==before, etc.
        // Non-standard variants on GEDCOM initial keywords
        private static Dictionary<string, KeyW> initialNonStdKeyLookup = new Dictionary<string, KeyW>()
        {
            {"BEF.", KeyW.Before},
            {"AFT.", KeyW.After},
            {"ABT.", KeyW.DontCare},
            {"EST.", KeyW.DontCare},
            {"CAL.", KeyW.DontCare},
            {"INT.", KeyW.DontCare},
            {"BETWEEN", KeyW.Between},
            {"BEFORE", KeyW.Before},
            {"AFTER", KeyW.After},
            {"ABOUT", KeyW.DontCare}, // BROSKEEP?
            {"CIRCA", KeyW.DontCare},
            {"C.", KeyW.DontCare}, // BROSKEEP ?
            {"AB.", KeyW.DontCare}, // personal
            {"MAYBE", KeyW.DontCare}, // personal
        };

        private static Dictionary<string, KeyW> secondKeyLookup = new Dictionary<string, KeyW>()
        {
            {"AND", KeyW.Between},
            {"TO", KeyW.From}
        };

        private static Dictionary<string, Era> eraLookup = new Dictionary<string, Era>
        {
            {"B.C.", Era.BC}
        };

        private static Dictionary<string, Era> nonStdEraLookup = new Dictionary<string, Era>
        {
            {"BC", Era.BC},
            {"BCE", Era.BC},
            {"B.C.E.", Era.BC},
            {"AD", Era.AD},
            {"A.D.", Era.AD},
            {"CE", Era.AD},
            {"C.E.", Era.AD},
        };

        static EventDateParse()
        {
        }

        private static Cal CheckCalendar(Context ctx)
        {
            Token tok = ctx.LookAhead();
            Cal calen = Cal.Greg;
            if (tok.type == TokType.CALEN)
            {
                if (!calLookup.TryGetValue(ctx.getString(), out calen))
                    calen = Cal.Fut;
                ctx.Consume();
            }
            return calen;
        }

        private static KeyW CheckInitialKeyword(Context ctx)
        {
            // TODO non-standard match needs flagging mechanism

            Token tok = ctx.LookAhead();
            if (tok.type != TokType.WORD)
                return KeyW.None;
            KeyW keyw;
            if (initialKeyLookup.TryGetValue(ctx.getString(), out keyw))
            {
                ctx.Consume();
                return keyw;
            }
            if (initialNonStdKeyLookup.TryGetValue(ctx.getString(), out keyw))
            {
                ctx.Consume();
                return keyw;
            }
            return KeyW.None;
        }

        private static KeyW CheckSecondKeyword(Context ctx)
        {
            // TODO non-standard match needs flagging mechanism

            Token tok = ctx.LookAhead();
            if (tok.type != TokType.WORD)
                return KeyW.None;
            KeyW keyw;
            if (secondKeyLookup.TryGetValue(ctx.getString(), out keyw))
            {
                ctx.Consume();
                return keyw;
            }
            // TODO any non-standard 2d key? "&"? "-"?
            //if (initialNonStdKeyLookup.TryGetValue(ctx.getString(), out keyw))
            //{
            //    ctx.Consume();
            //    return keyw;
            //}
            return KeyW.None;
        }

        private static Era CheckEra(Context ctx)
        {
            // TODO non-standard match needs flagging mechanism

            Token tok = ctx.LookAhead();
            if (tok.type != TokType.WORD)
                return Era.None;
            Era era;
            if (eraLookup.TryGetValue(ctx.getString(), out era))
            {
                ctx.Consume();
                return era;
            }
            if (nonStdEraLookup.TryGetValue(ctx.getString(), out era))
            {
                ctx.Consume();
                return era;
            }
            return Era.None;
        }

        private class Context
        {
            private List<Token> toks;
            private int tokDex;
            private int tokLen;
            public string datestr;
            public GEDDate gd;
            public GEDDate gd2;

            public Context( string _str, List<Token> _toks )
            {
                datestr = _str;
                toks = _toks;
                tokDex = 0;
                tokLen = toks.Count;
            }

            public Token LookAhead(int far=0)
            {
                if (tokDex+far >= tokLen)
                    return new Token {type = TokType.EOF};
                return toks[tokDex+far];
            }

            public Token Consume()
            {
                tokDex++;
                return LookAhead();
            }

            public string getString()
            {
                return toks[tokDex].getString(datestr);
            }

            public int getInt()
            {
                return toks[tokDex].getInt(datestr);
            }

            public void SetBC(bool isbc, bool second = false)
            {
                if (second)
                    gd2.IsBC = isbc;
                else
                    gd.IsBC = isbc;
            }

            private void Set(int day, int mon, int year, GEDDate.Types type, ref GEDDate tomod)
            {
                tomod.Day = day; // TODO may be invalid
                tomod.Month = mon;
                tomod.Year = year; // TODO may be invalid
                tomod.Type = type;
            }

            public void Set(int day, int mon, int year, GEDDate.Types type, bool second)
            {
                if (second)
                    Set(day, mon, year, type, ref gd2);
                else
                    Set(day, mon, year, type, ref gd);
            }
        }

        public static GEDDate DateParser(string datestr)
        {
            Context ctx = new Context(datestr, new DateTokens().Tokenize(datestr));

            // TODO this is not standard, but some programs do this
            Cal calen = CheckCalendar(ctx);
            if (calen != Cal.Greg) // TODO other calendar support
                return new GEDDate(GEDDate.Types.Unknown);

            // TODO grabbing keyword but doing nothing
            KeyW initKeyword = CheckInitialKeyword(ctx);

            ctx.gd = new GEDDate();
            if (!parseDate(ref ctx, calen))
                return ctx.gd;

            // check for an era
            Era era = CheckEra(ctx);
            ctx.SetBC(era == Era.BC);

            KeyW secondKeyword = CheckSecondKeyword(ctx);
            if (secondKeyword != KeyW.None)
            {
                ctx.gd2 = new GEDDate();
                // expecting a second date
                if (!parseDate(ref ctx, calen, second:true))
                    return new GEDDate(GEDDate.Types.Unknown); // TODO track/note issues
                Era era2 = CheckEra(ctx);
                ctx.SetBC(era2 == Era.BC, second:true);
            }

            // Still have unparsed stuff must be problem
            if (ctx.LookAhead().type != TokType.EOF)
                return new GEDDate(GEDDate.Types.Unknown); // TODO track/note issues

            MakeJulianDayRange(ctx, initKeyword, secondKeyword);

            // TODO merge ctx.gd2 into ctx.gd
            return ctx.gd;
        }

        private static bool parseDate(ref Context ctx, Cal calen, bool second=false)
        {
            // TODO according to standard, the calendar escape can go on each date, e.g. "AFT @#DJULIAN@ 1898"

            // parse a date.
            // TODO dd/mm/yyyy
            // TODO dd-mm-yyyy
            // day mon year : num word num
            // mon year : word num
            // year : num 
            int day = -1;
            int mon = -1;
            int year = -1;

            Token tok = ctx.LookAhead();
            if (tok.type == TokType.WORD)
            {
                // mon year?
                if (!getMonth(ctx.getString(), calen, ref mon))
                    return false; // Not a known month
                tok = ctx.Consume();
                if (tok.type != TokType.NUM)
                    return false; // no year following
                year = ctx.getInt();
                ctx.Consume();
                ctx.Set(-1,mon,year,GEDDate.Types.Range,second);
                return true;
            }

            if (tok.type != TokType.NUM)
            {
                return false; // Dunno what we got
            }

            // day mon year OR year
            GEDDate.Types newType;
            if (ctx.LookAhead(1).type == TokType.WORD)
            {
                // day mon year
                day = ctx.getInt();
                ctx.Consume();
                if (!getMonth(ctx.getString(), calen, ref mon))
                {
                    // Not a known month - might be a second keyword; assume YEAR
                    year = day;
                    day = -1;
                    mon = -1;
                    newType = GEDDate.Types.Range;
                }
                else
                {
                    tok = ctx.Consume();
                    if (tok.type != TokType.NUM)
                        return false; // Not a year, invalid
                    year = ctx.getInt();
                    ctx.Consume();
                    newType = GEDDate.Types.Exact;
                }
            }
            else
            {
                year = ctx.getInt();
                ctx.Consume();
                newType = GEDDate.Types.Range;
            }
            ctx.Set(day, mon, year, newType, second);
            return true;
        }

        private static bool getMonth(string str, Cal calen, ref int mon)
        {
            // TODO swap month lookup based on calendar
            // TODO may find month match in list for different calendar - change calendar?
            // TODO non-standard match needs flagging mechanism

            string str2 = str.Substring(0, 3);
            int monNum;
            if (gregMonLookup.TryGetValue(str2, out monNum))
            {
                mon = monNum;
                return true;
            }
            if (gregMonNonStdLookup.TryGetValue(str, out monNum))
            {
                mon = monNum;
                return true;
            }

            return false;
       }

        private static int ToJulianDay(int d, int m, int y, bool isBC)
        {
            // there's no year 0 - anything B.C. has to be shifted
            if (isBC)
                y = -y + 1;

            // Communications of the ACM by Henry F. Fliegel and Thomas C. Van Flandern entitled 
            // ``A Machine Algorithm for Processing Calendar Dates''. 
            // CACM, volume 11, number 10, October 1968, p. 657.  

            return (1461 * (y + 4800 + (m - 14) / 12)) / 4 +
                   (367 * (m - 2 - 12 * ((m - 14) / 12))) / 12 -
                   (3 * ((y + 4900 + (m - 14) / 12) / 100)) / 4 +
                   d - 32075;
        }

        private static long StartToJulian(GEDDate gd)
        {
            int m = gd.Month == -1 ? 1 : gd.Month;
            int d = gd.Day == -1 ? 1 : gd.Day;
            int y = gd.Year;
            return ToJulianDay(d, m, y, gd.IsBC);
        }

        private static bool isLeap(int year)
        {
            return ((year % 4 == 0) && ((year % 100 != 0) || (year % 400 == 0)));
        }

        private static int MonthEnd(int mon, int yr)
        {
            int [] monthDays = {0,31,28,31,30,31,30,31,31,30,31,30,31};
            if (mon == 2 && isLeap(yr))
                return 29;
            return monthDays[mon];
        }

        private static long EndToJulian(GEDDate gd)
        {
            int y = gd.Year;
            int m = gd.Month == -1 ? 12 : gd.Month;
            int d = gd.Day == -1 ? MonthEnd(m, y) : gd.Day;
            return ToJulianDay(d, m, y, gd.IsBC);
        }

        // No key, exact date: date to julian, range=1 [Exact]
        // No key, range date, no month: beg of year to julian, range=365/366 [Between][Range] [issue: leap] 
        // No key, range date, month: beg of month to julian, range=month length [Between][Range] [issue: leap] 
        // firstKey: (no secondkey)
        //   before - beg of date to julian [Before][Range]
        //   after -  end of date to julian [After][Range]
        //   from  -  beg of date to julian [After][Period]
        //   initialTo - end of date to julian [before][Period]
        // firstkey with second key:
        //   from - beg of date1 to end of date2 [Between][Period]
        //   between - beg of date1 to end of date2 [between][Range]
        private static void MakeJulianDayRange(Context ctx, KeyW firstKey, KeyW secondKey)
        {
            long jdn = 0;
            long range = 0;
            GEDDate.Types finalType = ctx.gd.Type;
            switch (firstKey)
            {
                case KeyW.None:
                case KeyW.DontCare:
                    jdn = StartToJulian(ctx.gd);
                    long jdn2 = jdn + 1;
                    if (ctx.gd.Type != GEDDate.Types.Exact)
                        jdn2 = EndToJulian(ctx.gd);
                    range = jdn2 - jdn;
                    // NOTE: not changing initial type finalType = GEDDate.Types.Range;
                    break;

                case KeyW.Before:
                    jdn = StartToJulian(ctx.gd);
                    finalType = GEDDate.Types.Before; // TODO mark as range?
                    break;

                case KeyW.After:
                    jdn = EndToJulian(ctx.gd);
                    finalType = GEDDate.Types.After; // TODO mark as range?
                    break;

                case KeyW.InitialTo:
                    jdn = EndToJulian(ctx.gd);
                    finalType = GEDDate.Types.Before; // TODO mark as period?
                    break;

                case KeyW.From:
                    jdn = StartToJulian(ctx.gd);
                    if (secondKey == KeyW.None)
                        finalType = GEDDate.Types.After; // TODO mark as range?
                    else
                    {
                        jdn2 = EndToJulian(ctx.gd2);
                        range = jdn2 - jdn;
                        finalType = GEDDate.Types.Range; // TODO mark as period?
                    }
                    break;

                case KeyW.Between:
                    jdn = StartToJulian(ctx.gd);
                    jdn2 = EndToJulian(ctx.gd2);
                    range = jdn2 - jdn;
                    finalType = GEDDate.Types.Range; // TODO mark as range?
                    break;
            }
            ctx.gd.JDN = jdn;
            ctx.gd.JDR = range;
            ctx.gd.Type = finalType;
        }
    }
}
