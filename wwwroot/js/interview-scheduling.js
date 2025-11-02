// Interview Scheduling with Business Hours Validation
class InterviewScheduler {
    constructor() {
        this.businessHours = {
            start: '09:00',
            end: '16:00',
            days: ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday']
        };
        this.initializeEventListeners();
    }

    initializeEventListeners() {
        // Date picker validation
        const dateInput = document.getElementById('interviewDate');
        if (dateInput) {
            dateInput.addEventListener('change', (e) => this.validateDate(e.target.value));
        }

        // Time picker validation
        const timeInput = document.getElementById('interviewTime');
        if (timeInput) {
            timeInput.addEventListener('change', (e) => this.validateTime(e.target.value));
        }

        // Duration validation
        const durationInput = document.getElementById('durationMinutes');
        if (durationInput) {
            durationInput.addEventListener('change', (e) => this.validateDuration(e.target.value));
        }

        // Auto-schedule button
        const autoScheduleBtn = document.getElementById('autoScheduleBtn');
        if (autoScheduleBtn) {
            autoScheduleBtn.addEventListener('click', (e) => this.autoScheduleInterviews(e));
        }
    }

    validateDate(selectedDate) {
        const date = new Date(selectedDate);
        const dayOfWeek = date.getDay();
        const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
        
        if (!this.businessHours.days.includes(dayNames[dayOfWeek])) {
            this.showError('Please select a weekday (Monday-Friday)');
            return false;
        }

        if (date <= new Date()) {
            this.showError('Please select a future date');
            return false;
        }

        this.clearError();
        return true;
    }

    validateTime(selectedTime) {
        if (!selectedTime) return false;

        const time = selectedTime.split(':');
        const hours = parseInt(time[0]);
        const minutes = parseInt(time[1]);
        
        const businessStart = parseInt(this.businessHours.start.split(':')[0]);
        const businessEnd = parseInt(this.businessHours.end.split(':')[0]);

        if (hours < businessStart || hours >= businessEnd) {
            this.showError(`Time must be between ${this.businessHours.start} and ${this.businessHours.end}`);
            return false;
        }

        this.clearError();
        return true;
    }

    validateDuration(duration) {
        const durationNum = parseInt(duration);
        
        if (durationNum < 30 || durationNum > 120) {
            this.showError('Duration must be between 30 and 120 minutes');
            return false;
        }

        this.clearError();
        return true;
    }

    async getAvailableTimeSlots(date, duration, recruiterName) {
        try {
            const response = await fetch(`/RecruitmentAdmin/GetAvailableTimeSlots?date=${date}&durationMinutes=${duration}&recruiterName=${encodeURIComponent(recruiterName)}`);
            const data = await response.json();
            
            if (data.success) {
                this.displayTimeSlots(data.timeSlots);
                return data.timeSlots;
            } else {
                this.showError('Error retrieving available time slots');
                return [];
            }
        } catch (error) {
            console.error('Error fetching time slots:', error);
            this.showError('Error retrieving available time slots');
            return [];
        }
    }

    displayTimeSlots(timeSlots) {
        const container = document.getElementById('timeSlotsContainer');
        if (!container) return;

        container.innerHTML = '';

        if (timeSlots.length === 0) {
            container.innerHTML = '<p class="text-muted">No available time slots for the selected date.</p>';
            return;
        }

        const slotsHtml = timeSlots.map(slot => {
            const statusClass = slot.isAvailable ? 'success' : 'danger';
            const statusText = slot.isAvailable ? 'Available' : 'Booked';
            
            return `
                <div class="col-md-3 mb-2">
                    <div class="card border-${statusClass}">
                        <div class="card-body text-center">
                            <h6 class="card-title">${this.formatTime(slot.startTime)} - ${this.formatTime(slot.endTime)}</h6>
                            <span class="badge bg-${statusClass}">${statusText}</span>
                            ${slot.conflictReason ? `<small class="text-muted d-block">${slot.conflictReason}</small>` : ''}
                        </div>
                    </div>
                </div>
            `;
        }).join('');

        container.innerHTML = `
            <div class="row">
                ${slotsHtml}
            </div>
        `;
    }

    async autoScheduleInterviews(event) {
        event.preventDefault();
        
        const form = event.target.closest('form');
        const formData = new FormData(form);
        
        const selectedCandidates = Array.from(document.querySelectorAll('input[name="selectedApplicationIds"]:checked'))
            .map(input => parseInt(input.value));

        if (selectedCandidates.length === 0) {
            this.showError('Please select at least one candidate');
            return;
        }

        const jobId = formData.get('jobId');
        const startDate = formData.get('interviewDate');
        const durationMinutes = formData.get('durationMinutes');
        const recruiterName = formData.get('recruiterName');
        const recruiterEmail = formData.get('recruiterEmail');

        // Validate inputs
        if (!this.validateDate(startDate) || !this.validateDuration(durationMinutes)) {
            return;
        }

        try {
            const response = await fetch('/RecruitmentAdmin/AutoScheduleWithIntervals', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify({
                    jobId: parseInt(jobId),
                    selectedApplicationIds: selectedCandidates,
                    startDate: startDate,
                    durationMinutes: parseInt(durationMinutes),
                    recruiterName: recruiterName,
                    recruiterEmail: recruiterEmail
                })
            });

            const result = await response.json();
            
            if (result.success) {
                this.showSuccess(result.message);
                
                if (result.warnings && result.warnings.length > 0) {
                    this.showWarning(result.warnings.join('; '));
                }
                
                // Refresh the page after a short delay
                setTimeout(() => {
                    window.location.reload();
                }, 2000);
            } else {
                this.showError(result.error || 'Error scheduling interviews');
            }
        } catch (error) {
            console.error('Error auto-scheduling interviews:', error);
            this.showError('Error scheduling interviews');
        }
    }

    formatTime(dateTimeString) {
        const date = new Date(dateTimeString);
        return date.toLocaleTimeString('en-US', { 
            hour: '2-digit', 
            minute: '2-digit',
            hour12: false 
        });
    }

    showError(message) {
        this.showAlert(message, 'danger');
    }

    showSuccess(message) {
        this.showAlert(message, 'success');
    }

    showWarning(message) {
        this.showAlert(message, 'warning');
    }

    showAlert(message, type) {
        const alertContainer = document.getElementById('alertContainer') || this.createAlertContainer();
        
        const alertId = 'alert-' + Date.now();
        const alertHtml = `
            <div id="${alertId}" class="alert alert-${type} alert-dismissible fade show" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;
        
        alertContainer.insertAdjacentHTML('beforeend', alertHtml);
        
        // Auto-dismiss after 5 seconds
        setTimeout(() => {
            const alert = document.getElementById(alertId);
            if (alert) {
                alert.remove();
            }
        }, 5000);
    }

    createAlertContainer() {
        const container = document.createElement('div');
        container.id = 'alertContainer';
        container.className = 'position-fixed top-0 end-0 p-3';
        container.style.zIndex = '1050';
        document.body.appendChild(container);
        return container;
    }

    clearError() {
        const alerts = document.querySelectorAll('.alert-danger');
        alerts.forEach(alert => alert.remove());
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    new InterviewScheduler();
});
