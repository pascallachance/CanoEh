import React from 'react';

interface FlagIconProps {
    language: 'en' | 'fr';
    className?: string;
}

const flagStyles: React.CSSProperties = {
    display: 'inline-block',
    verticalAlign: 'middle',
    marginRight: '6px'
};

function FlagIcon({ language, className = '' }: FlagIconProps) {
    const ariaLabel = language === 'en' ? 'English' : 'Quebec French';
    
    if (language === 'en') {
        // Canadian Flag - using official flag image from Wikipedia
        return (
            <img
                src="http://upload.wikimedia.org/wikipedia/en/thumb/c/cf/Flag_of_Canada.svg/960px-Flag_of_Canada.svg.png?20190402205958"
                alt={ariaLabel}
                className={className}
                width="24"
                height="18"
                style={flagStyles}
                role="img"
                aria-label={ariaLabel}
            />
        );
    } else {
        // Quebec Flag - using official flag image from Wikipedia
        return (
            <img
                src="https://upload.wikimedia.org/wikipedia/commons/thumb/5/5f/Flag_of_Quebec.svg/960px-Flag_of_Quebec.svg.png?20250902230651"
                alt={ariaLabel}
                className={className}
                width="24"
                height="18"
                style={flagStyles}
                role="img"
                aria-label={ariaLabel}
            />
        );
    }
}

export default FlagIcon;
