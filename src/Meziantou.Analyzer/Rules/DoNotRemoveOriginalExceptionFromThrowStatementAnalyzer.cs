﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Meziantou.Analyzer.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotRemoveOriginalExceptionFromThrowStatementAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            RuleIdentifiers.DoNotRemoveOriginalExceptionFromThrowStatement,
            title: "Do not remove original exception",
            messageFormat: "Do not remove original exception",
            RuleCategories.Usage,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "",
            helpLinkUri: RuleIdentifiers.GetHelpUri(RuleIdentifiers.DoNotRemoveOriginalExceptionFromThrowStatement));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            var values = Enum.GetValues(typeof(OperationKind)).Cast<OperationKind>().ToArray();

            context.RegisterOperationAction(Analyze, OperationKind.Throw);
        }

        private void Analyze(OperationAnalysisContext context)
        {
            var operation = (IThrowOperation)context.Operation;
            if (operation.Exception == null)
                return;

            var localReferenceOperation = operation.Exception as ILocalReferenceOperation;
            if (localReferenceOperation == null)
                return;

            var catchOperation = operation.Ancestors().OfType<ICatchClauseOperation>().FirstOrDefault();
            if (catchOperation == null)
                return;

            if (catchOperation.Locals.Contains(localReferenceOperation.Local))
            {
                context.ReportDiagnostic(Diagnostic.Create(s_rule, operation.Syntax.GetLocation()));
            }
        }
    }
}
