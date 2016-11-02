﻿using SharpGEDParser.Model;
using System.Collections.Generic;

namespace SharpGEDParser.Parser
{
    class IndiAssoParse : StructParser
    {
        private static readonly Dictionary<string, TagProc> tagDict = new Dictionary<string, TagProc>()
        {
            {"RELA", relaProc},
            {"NOTE", noteProc},
            {"SOUR", sourProc}
        };

        private static void relaProc(StructParseContext context, int linedex, char level)
        {
            var rec = context.Parent as AssoRec;
            rec.Relation = context.Remain;
        }

        public static AssoRec AssoParse(GedRecParse.ParseContext2 ctx)
        {
            var asso = new AssoRec();

            string xref;
            string extra;
            StructParser.parseXrefExtra(ctx.Remain, out xref, out extra);

            if (string.IsNullOrEmpty(xref))
            {
                UnkRec err = new UnkRec();
                err.Error = "Missing/unterminated identifier: " + ctx.Tag;
                err.Beg = err.End = ctx.Begline;
                ctx.Parent.Errors.Add(err);
            }
            else
            {
                asso.Ident = xref;
            }
            StructParseContext ctx2 = new StructParseContext(ctx, asso);
            StructParse(ctx2, tagDict);
            ctx.Endline = ctx2.Endline;
            return asso;
            // TODO validate relation specified
            // TODO validate ident existance
        }
    }
}
