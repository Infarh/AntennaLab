using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using MathCore.MathParser;

namespace ArrayFactor.Service.AmplitudeDistributions;

internal class UserDistribution : Distribution
{
    private static readonly ExpressionParser __Parser = new();

    private MathExpression? _DistributionExpression;
    private readonly ObservableCollection<ExpressionVariable> _Variables = new();

    public string? DestributionExpression { get => Get<string>(); set => Set(value); }

    public DistributionValue[] DistributionPhi0 => GetDistribution();
    public DistributionValue[] DistributionPhi90 => GetDistribution(Math.PI / 2);

    public ReadOnlyObservableCollection<ExpressionVariable> Variables { get; }

    public UserDistribution()
    {
        PropertyChanged_AddHandler(nameof(DestributionExpression), CreateExpression);
        PropertyDependence_Add(nameof(DestributionExpression), nameof(DistributionPhi0), nameof(DistributionPhi90));

        Variables                    =  new ReadOnlyObservableCollection<ExpressionVariable>(_Variables);
        _Variables.CollectionChanged += OnVariablesCollectionChanged;
    }

    private void OnVariablesCollectionChanged(object? Sender, NotifyCollectionChangedEventArgs E)
    {
        if (E.Action != NotifyCollectionChangedAction.Add || E.NewItems is null) return;
        foreach (var v in E.NewItems.OfType<ExpressionVariable>().Where(v => v.Name != "x" && v.Name != "y"))
            v.PropertyChanged += OnVariablePropertyChanged;
    }

    private void OnVariablePropertyChanged(object? Sender, PropertyChangedEventArgs E)
    {
        OnPropertyChanged(nameof(DistributionPhi0));
        OnPropertyChanged(nameof(DistributionPhi90));
    }

    private void CreateExpression()
    {
        var             expr_str = DestributionExpression;
        MathExpression? expr;
        _DistributionExpression = expr = expr_str is { Length: > 0 } ? __Parser.Parse(expr_str) : null;
        _Variables.Clear();
        expr?.Variable.Where(v => v.Name != "x" && v.Name != "y").AddTo(_Variables);
    }

    /// <inheritdoc />
    public override double Value(double x, double y, double z)
    {
        var expr = _DistributionExpression;
        try
        {
            return expr?.Compute(x, y) ?? double.NaN;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            _DistributionExpression = null;
            return double.NaN;
        }
    }
}