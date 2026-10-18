using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using MathCore.MathParser;

namespace ArrayFactor.Service.AmplitudeDistributions;

internal class UserDistribution : Distribution
{
    private static readonly ExpressionParser __Parser = new();

    private MathExpression? _DistributionExpression;
    private readonly ObservableCollection<ExpressionVariable> _Variables = [];

    public string? DistributionExpression { get => Get<string>(); set => Set(value); }

    public DistributionValue[] DistributionPhi0 => GetDistribution();
    public DistributionValue[] DistributionPhi90 => GetDistribution(Math.PI / 2);

    public ReadOnlyObservableCollection<ExpressionVariable> Variables { get; }

    public UserDistribution()
    {
        PropertyChanged_AddHandler(nameof(DistributionExpression), CreateExpression);
        PropertyDependence_Add(nameof(DistributionExpression), nameof(DistributionPhi0), nameof(DistributionPhi90));

        Variables                    =  new(_Variables);
        _Variables.CollectionChanged += OnVariablesCollectionChanged;
    }

    private void OnVariablesCollectionChanged(object? Sender, NotifyCollectionChangedEventArgs E)
    {
        if (E.OldItems is not null)
            foreach (var v in E.OldItems.OfType<ExpressionVariable>())
                v.PropertyChanged -= OnVariablePropertyChanged;

        if (E.Action == NotifyCollectionChangedAction.Reset) return;

        if (E.NewItems is null) return;
        foreach (var v in E.NewItems.OfType<ExpressionVariable>().Where(v => v.Name is not "x" and not "y"))
            v.PropertyChanged += OnVariablePropertyChanged;
    }

    private void UnsubscribeVariables()
    {
        foreach (var v in _Variables) v.PropertyChanged -= OnVariablePropertyChanged;
    }

    private void OnVariablePropertyChanged(object? Sender, PropertyChangedEventArgs E)
    {
        OnPropertyChanged(nameof(DistributionPhi0));
        OnPropertyChanged(nameof(DistributionPhi90));
    }

    private void CreateExpression()
    {
        var expression_text = DistributionExpression;
        try
        {
            var expression = expression_text is { Length: > 0 } ? __Parser.Parse(expression_text) : null;
            UnsubscribeVariables();
            _DistributionExpression = expression;
            _Variables.Clear();
            expression?.Variable.Where(v => v.Name is not "x" and not "y").AddTo(_Variables);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            UnsubscribeVariables();
            _DistributionExpression = null;
            _Variables.Clear();
        }
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