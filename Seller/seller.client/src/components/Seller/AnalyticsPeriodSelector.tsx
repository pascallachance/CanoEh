import './AnalyticsSection.css';

export type PeriodType = '7d' | '30d' | '90d' | '1y';

export interface AnalyticsPeriodSelectorProps {
    selectedPeriod: PeriodType;
    onPeriodChange: (period: PeriodType) => void;
}

function AnalyticsPeriodSelector({ selectedPeriod, onPeriodChange }: AnalyticsPeriodSelectorProps) {
    return (
        <div className="analytics-period-selector">
            <label className="analytics-period-label">Time Period:</label>
            <select 
                value={selectedPeriod}
                onChange={(e) => onPeriodChange(e.target.value as PeriodType)}
                className="analytics-period-select"
            >
                <option value="7d">Last 7 Days</option>
                <option value="30d">Last 30 Days</option>
                <option value="90d">Last 90 Days</option>
                <option value="1y">Last Year</option>
            </select>
        </div>
    );
}

export default AnalyticsPeriodSelector;