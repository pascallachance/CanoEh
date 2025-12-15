import { useState, type KeyboardEvent, type ChangeEvent } from 'react';
import './TagInput.css';

interface TagInputProps {
    tags: string[];
    onTagsChange: (tags: string[]) => void;
    placeholder?: string;
    label?: string;
    id?: string;
}

function TagInput({ tags, onTagsChange, placeholder = 'Type and press Enter to add', label, id }: TagInputProps) {
    const [inputValue, setInputValue] = useState('');

    const handleInputChange = (e: ChangeEvent<HTMLInputElement>) => {
        setInputValue(e.target.value);
    };

    const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
        if (e.key === 'Enter' && inputValue.trim()) {
            e.preventDefault();
            // Add the new tag if it's not already in the list (case-insensitive)
            if (!tags.some(tag => tag.toLowerCase() === inputValue.trim().toLowerCase())) {
                onTagsChange([...tags, inputValue.trim()]);
            }
            setInputValue('');
        } else if (e.key === 'Backspace' && !inputValue && tags.length > 0) {
            // Remove the last tag if backspace is pressed on empty input
            e.preventDefault();
            onTagsChange(tags.slice(0, -1));
        }
    };

    const removeTag = (indexToRemove: number) => {
        onTagsChange(tags.filter((_, index) => index !== indexToRemove));
    };

    return (
        <div className="tag-input-container">
            {label && <label htmlFor={id} className="tag-input-label">{label}</label>}
            <div className="tag-input-wrapper">
                <div className="tags-list">
                    {tags.map((tag, index) => (
                        <span key={index} className="tag">
                            {tag}
                            <button
                                type="button"
                                onClick={() => removeTag(index)}
                                className="tag-remove-btn"
                                aria-label={`Remove ${tag}`}
                            >
                                ×
                            </button>
                        </span>
                    ))}
                </div>
                <input
                    id={id}
                    type="text"
                    value={inputValue}
                    onChange={handleInputChange}
                    onKeyDown={handleKeyDown}
                    placeholder={placeholder}
                    className="tag-input-field"
                />
            </div>
        </div>
    );
}

export default TagInput;
