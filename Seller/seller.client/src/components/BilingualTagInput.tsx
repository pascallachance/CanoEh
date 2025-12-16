import { useState, useRef, type KeyboardEvent, type ChangeEvent } from 'react';
import './BilingualTagInput.css';

export interface BilingualValue {
    en: string;
    fr: string;
}

interface BilingualTagInputProps {
    values: BilingualValue[];
    onValuesChange: (values: BilingualValue[]) => void;
    placeholderEn?: string;
    placeholderFr?: string;
    labelEn?: string;
    labelFr?: string;
    id?: string;
}

function BilingualTagInput({ 
    values, 
    onValuesChange, 
    placeholderEn = 'Type English value and press Enter',
    placeholderFr = 'Type French value and press Enter',
    labelEn = 'Values (English)',
    labelFr = 'Values (French)',
    id 
}: BilingualTagInputProps) {
    const [inputValueEn, setInputValueEn] = useState('');
    const [inputValueFr, setInputValueFr] = useState('');
    const [error, setError] = useState('');
    const inputRefEn = useRef<HTMLInputElement>(null);
    const inputRefFr = useRef<HTMLInputElement>(null);

    const handleInputChangeEn = (e: ChangeEvent<HTMLInputElement>) => {
        setInputValueEn(e.target.value);
        if (error) setError('');
    };

    const handleInputChangeFr = (e: ChangeEvent<HTMLInputElement>) => {
        setInputValueFr(e.target.value);
        if (error) setError('');
    };

    const addValue = () => {
        const trimmedEn = inputValueEn.trim();
        const trimmedFr = inputValueFr.trim();

        // Both values must be provided
        if (!trimmedEn || !trimmedFr) {
            setError('Both English and French values are required');
            return;
        }

        // Check for duplicates (case-insensitive)
        const isDuplicateEn = values.some(v => v.en.toLowerCase() === trimmedEn.toLowerCase());
        const isDuplicateFr = values.some(v => v.fr.toLowerCase() === trimmedFr.toLowerCase());

        if (isDuplicateEn || isDuplicateFr) {
            setError('This value already exists');
            return;
        }

        // Add the paired value
        onValuesChange([...values, { en: trimmedEn, fr: trimmedFr }]);
        setInputValueEn('');
        setInputValueFr('');
        setError('');
        
        // Focus the English input field to be ready for the next value
        // Use setTimeout to ensure the focus occurs after the DOM has been updated
        setTimeout(() => {
            if (inputRefEn.current) {
                inputRefEn.current.focus();
            }
        }, 0);
    };

    const handleRemoveLastValue = () => {
        if (!inputValueEn && !inputValueFr && values.length > 0) {
            onValuesChange(values.slice(0, -1));
        }
    };

    const handleKeyDownEn = (e: KeyboardEvent<HTMLInputElement>) => {
        if (e.key === 'Enter' || e.key === 'Tab') {
            e.preventDefault();
            const trimmedEn = inputValueEn.trim();
            const trimmedFr = inputValueFr.trim();
            
            // If EN has value but FR is empty, move focus to FR input
            if (trimmedEn && !trimmedFr) {
                if (inputRefFr.current) {
                    inputRefFr.current.focus();
                }
            } else {
                // If both have values or both are empty, try to add the value
                addValue();
            }
        } else if (e.key === 'Backspace' && !inputValueEn) {
            // Only prevent default and remove last value when input is empty
            e.preventDefault();
            handleRemoveLastValue();
        }
    };

    const handleKeyDownFr = (e: KeyboardEvent<HTMLInputElement>) => {
        if (e.key === 'Enter' || e.key === 'Tab') {
            e.preventDefault();
            const trimmedEn = inputValueEn.trim();
            const trimmedFr = inputValueFr.trim();
            
            // If FR has value but EN is empty, move focus to EN input
            if (trimmedFr && !trimmedEn) {
                if (inputRefEn.current) {
                    inputRefEn.current.focus();
                }
            } else {
                // If both have values or both are empty, try to add the value
                addValue();
            }
        } else if (e.key === 'Backspace' && !inputValueFr) {
            // Only prevent default and remove last value when input is empty
            e.preventDefault();
            handleRemoveLastValue();
        }
    };

    const removeValue = (indexToRemove: number) => {
        onValuesChange(values.filter((_, index) => index !== indexToRemove));
    };

    const moveValueUp = (index: number) => {
        if (index === 0) return;
        const newValues = [...values];
        [newValues[index - 1], newValues[index]] = [newValues[index], newValues[index - 1]];
        onValuesChange(newValues);
    };

    const moveValueDown = (index: number) => {
        if (index === values.length - 1) return;
        const newValues = [...values];
        [newValues[index], newValues[index + 1]] = [newValues[index + 1], newValues[index]];
        onValuesChange(newValues);
    };

    return (
        <div className="bilingual-tag-input-container">
            <div className="bilingual-tag-input-columns">
                <div className="bilingual-tag-input-column">
                    <label htmlFor={`${id}-en`} className="bilingual-tag-input-label">{labelEn}</label>
                    <div className="bilingual-tag-input-wrapper">
                        <div className="bilingual-tags-list">
                            {values.map((value, index) => (
                                <div key={index} className="bilingual-tag-pair">
                                    <span className="bilingual-tag">
                                        {value.en}
                                    </span>
                                    <div className="bilingual-tag-actions">
                                        <button
                                            type="button"
                                            onClick={() => moveValueUp(index)}
                                            className="bilingual-tag-move-btn"
                                            aria-label={`Move ${value.en} up`}
                                            disabled={index === 0}
                                            title="Move up"
                                        >
                                            ↑
                                        </button>
                                        <button
                                            type="button"
                                            onClick={() => moveValueDown(index)}
                                            className="bilingual-tag-move-btn"
                                            aria-label={`Move ${value.en} down`}
                                            disabled={index === values.length - 1}
                                            title="Move down"
                                        >
                                            ↓
                                        </button>
                                        <button
                                            type="button"
                                            onClick={() => removeValue(index)}
                                            className="bilingual-tag-remove-btn"
                                            aria-label={`Remove ${value.en} and ${value.fr}`}
                                            title="Remove"
                                        >
                                            ×
                                        </button>
                                    </div>
                                </div>
                            ))}
                        </div>
                        <input
                            id={`${id}-en`}
                            type="text"
                            value={inputValueEn}
                            onChange={handleInputChangeEn}
                            onKeyDown={handleKeyDownEn}
                            placeholder={placeholderEn}
                            className="bilingual-tag-input-field"
                            ref={inputRefEn}
                        />
                    </div>
                </div>
                <div className="bilingual-tag-input-column">
                    <label htmlFor={`${id}-fr`} className="bilingual-tag-input-label">{labelFr}</label>
                    <div className="bilingual-tag-input-wrapper">
                        <div className="bilingual-tags-list">
                            {values.map((value, index) => (
                                <div key={index} className="bilingual-tag-pair">
                                    <span className="bilingual-tag">
                                        {value.fr}
                                    </span>
                                    <div className="bilingual-tag-actions">
                                        <button
                                            type="button"
                                            onClick={() => moveValueUp(index)}
                                            className="bilingual-tag-move-btn"
                                            aria-label={`Move ${value.fr} up`}
                                            disabled={index === 0}
                                            title="Move up"
                                        >
                                            ↑
                                        </button>
                                        <button
                                            type="button"
                                            onClick={() => moveValueDown(index)}
                                            className="bilingual-tag-move-btn"
                                            aria-label={`Move ${value.fr} down`}
                                            disabled={index === values.length - 1}
                                            title="Move down"
                                        >
                                            ↓
                                        </button>
                                        <button
                                            type="button"
                                            onClick={() => removeValue(index)}
                                            className="bilingual-tag-remove-btn"
                                            aria-label={`Remove ${value.en} and ${value.fr}`}
                                            title="Remove"
                                        >
                                            ×
                                        </button>
                                    </div>
                                </div>
                            ))}
                        </div>
                        <input
                            id={`${id}-fr`}
                            type="text"
                            value={inputValueFr}
                            onChange={handleInputChangeFr}
                            onKeyDown={handleKeyDownFr}
                            placeholder={placeholderFr}
                            className="bilingual-tag-input-field"
                            ref={inputRefFr}
                        />
                    </div>
                </div>
            </div>
            {error && (
                <div className="bilingual-tag-input-error" role="alert">
                    {error}
                </div>
            )}
            <div className="bilingual-tag-input-help">
                <p><strong>Note:</strong> Fill both English and French inputs, then press Enter to add a paired value. Both values are always added, edited, or removed together to maintain synchronization.</p>
            </div>
        </div>
    );
}

export default BilingualTagInput;
