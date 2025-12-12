import React from 'react';
import './StepIndicator.css';

interface StepIndicatorProps {
    currentStep: number;
    totalSteps: number;
    onStepClick?: (step: number) => void;
    completedSteps?: number[];
}

function StepIndicator({ currentStep, totalSteps, onStepClick, completedSteps = [] }: StepIndicatorProps) {
    const handleStepClick = (step: number) => {
        // Only allow clicking if a handler is provided (edit mode)
        if (onStepClick) {
            onStepClick(step);
        }
    };

    const getStepClass = (step: number): string => {
        if (step === currentStep) {
            return 'step active';
        }
        if (completedSteps.includes(step)) {
            return 'step completed';
        }
        return 'step';
    };

    const isClickable = (step: number): boolean => {
        // Step is clickable if we have a handler and the step is different from current step
        return !!onStepClick && step !== currentStep;
    };

    return (
        <div className="step-indicator">
            {Array.from({ length: totalSteps }, (_, index) => {
                const stepNumber = index + 1;
                const clickable = isClickable(stepNumber);
                
                return (
                    <div key={stepNumber} style={{ display: 'flex', alignItems: 'center' }}>
                        <span 
                            className={`${getStepClass(stepNumber)} ${clickable ? 'clickable' : ''}`}
                            onClick={() => clickable && handleStepClick(stepNumber)}
                            style={{ cursor: clickable ? 'pointer' : 'default' }}
                        >
                            {stepNumber}
                        </span>
                        {stepNumber < totalSteps && <span className="step-divider"></span>}
                    </div>
                );
            })}
        </div>
    );
}

export default StepIndicator;
