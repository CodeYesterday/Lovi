using CodeYesterday.Lovi.Models;
using Microsoft.AspNetCore.Components;
using Serilog.Events;
using Serilog.Parsing;
using System.Runtime.CompilerServices;
using System.Text;

namespace CodeYesterday.Lovi.Helper;

internal static class LogEventHtmlRenderer
{
    public static RenderFragment<LogItemModel> RenderedLogMessage => logItem => builder =>
    {
        builder.AddMarkupContent(0, Render(logItem.LogEvent.MessageTemplate, logItem.LogEvent.Properties, false));
    };

    public static RenderFragment<LogItemModel> RenderedMultiLineLogMessage => logItem => builder =>
    {
        builder.AddMarkupContent(0, Render(logItem.LogEvent.MessageTemplate, logItem.LogEvent.Properties, true));
    };

    public static string Render(MessageTemplate messageTemplate,
        IReadOnlyDictionary<string, LogEventPropertyValue> properties, bool multiLine, string? format = null,
        IFormatProvider? formatProvider = null)
    {
        var sb = new StringBuilder();
        using var textWriter = new StringWriter(sb);
        Render(messageTemplate, properties, textWriter, format, formatProvider);

        sb.Replace("\r", "");
        if (multiLine)
        {
            sb.Replace("\n", "<br/>");
        }
        else
        {
            sb.Replace("\n", " ");
        }
        return sb.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Render(MessageTemplate messageTemplate, IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output, string? format = null, IFormatProvider? formatProvider = null)
    {
        bool isLiteral = false;

        if (format != null)
        {
            isLiteral = format.Any(c => c == 'l');
        }

        foreach (var token in messageTemplate.Tokens)
        {
            if (token is TextToken tt)
            {
                RenderTextToken(tt, output);
            }
            else
            {
                var pt = (PropertyToken)token;
                RenderPropertyToken(pt, properties, output, formatProvider, isLiteral);
            }
        }
    }

    public static void RenderTextToken(TextToken tt, TextWriter output)
    {
        output.Write(tt.Text);
    }

    public static void RenderPropertyToken(PropertyToken pt, IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output, IFormatProvider? formatProvider, bool isLiteral)
    {
        if (!properties.TryGetValue(pt.PropertyName, out var propertyValue))
        {
            output.Write($"<span class=\"slm-missing-property\">{pt}</span>");
            return;
        }

        RenderValue(propertyValue, isLiteral, output, pt.Format, formatProvider);
    }

    public static string RenderValue(LogEventPropertyValue propertyValue, bool literal = false, string? format = null,
        IFormatProvider? formatProvider = null)
    {
        var sb = new StringBuilder();
        using var textWriter = new StringWriter(sb);
        RenderValue(propertyValue, literal, textWriter, format, formatProvider);
        return sb.ToString();
    }

    public static void RenderValue(LogEventPropertyValue propertyValue, bool literal, TextWriter output, string? format, IFormatProvider? formatProvider)
    {
        if (literal && propertyValue is ScalarValue { Value: string str })
        {
            output.Write($"<span class=\"slm-string-value\">\"{str}\"</span>");
        }
        else if (propertyValue is ScalarValue scalarValue)
        {
            if (scalarValue.Value is string str2)
            {
                output.Write($"<span class=\"slm-string-value\">\"{str2}\"</span>");
            }
            else if (scalarValue.Value is byte or short or ushort or int or uint or long or ulong or float or double or Decimal)
            {
                output.Write("<span class=\"slm-numeric-value\">");
                propertyValue.Render(output, format, formatProvider);
                output.Write("</span>");
            }
            else
            {
                output.Write("<span class=\"slm-scalar-value\">");
                propertyValue.Render(output, format, formatProvider);
                output.Write("</span>");
            }
        }
        else if (propertyValue is SequenceValue sequenceValue)
        {
            output.Write("<span class=\"slm-operator\">[</span>");
            bool first = true;
            foreach (var element in sequenceValue.Elements)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    output.Write("<span class=\"slm-operator\">, </span>");
                }
                RenderValue(element, literal, output, format, formatProvider);
            }
            output.Write("<span class=\"slm-operator\">]</span>");
        }
        else if (propertyValue is StructureValue structureValue)
        {
            output.Write("<span class=\"slm-operator\">{ </span>");
            bool first = true;
            foreach (var property in structureValue.Properties)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    output.Write("<span class=\"slm-operator\">, </span>");
                }
                output.Write($"<span class=\"slm-property-name\">{property.Name}</span>");
                output.Write("<span class=\"slm-operator\">: </span>");
                RenderValue(property.Value, literal, output, format, formatProvider);
            }
            output.Write("<span class=\"slm-operator\"> }</span>");
        }
        else
        {
            output.Write("<span class=\"slm-any-value\">");
            propertyValue.Render(output, format, formatProvider);
            output.Write("</span>");
        }
    }
}