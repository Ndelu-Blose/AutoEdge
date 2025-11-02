Use Case 01 – List a Vehicle

Use Case Name: List a Vehicle/Car
Actors: Admin
Brief Description: Admin adds a vehicle to the dealership’s inventory.
Trigger: Vehicle is ready to be added to inventory after workshop completion.
Preconditions: Vehicle has passed workshop inspection and is ready for listing.
Flow of Activities:

Admin uploads vehicle images.

Admin enters vehicle details (make, model, year, mileage, etc.).

Admin enters selling price.

Admin assigns vehicle status (Available/Reserved/Sold).
Postconditions: Vehicle is listed and visible in the inventory.

Use Case 02 – Browse Inventory

Actors: Customer (Buyer)
Brief Description: Customers can view, search, and filter available vehicles with personalized recommendations.
Trigger: Customer opens the “Browse Cars” page (website or mobile app).
Preconditions:

Customer is online (authentication optional).

Inventory is up-to-date with stock, specs, and pricing.
Flow of Activities:

Customer opens “Browse Inventory” page.

System displays available vehicles.

Customer applies filters (price, make, model, etc.).

System refreshes the list based on filters.

Customer selects a vehicle to view details.
Postconditions: Customer sees refined results or detailed vehicle info.

Use Case 03 – Select Vehicle

Actors: Customer
Brief Description: Customer selects a vehicle and views detailed information (features, specifications, pricing).
Trigger: Customer enters search criteria or clicks on a vehicle.
Preconditions: Customer has access to the online car platform.
Flow of Activities:

Customer enters search criteria.

System retrieves and displays search results.

Customer filters or compares vehicles.

Customer selects a specific vehicle to view details.
Postconditions: Customer has detailed information about the selected vehicle.

Use Case 04 – Submit Inquiry / Reserve / Buy

Actors: Customer
Brief Description: Customer can inquire about a vehicle, reserve it, or proceed with purchase.
Trigger: Customer clicks “Inquire”, “Reserve”, or “Buy Now” on a listing.
Preconditions:

Customer is logged in.

Vehicle is available.
Flow of Activities:

Customer selects a vehicle.

Customer chooses Inquire / Reserve / Buy.

If buying: uploads documents.

System initiates financing (if applicable).

Customer reviews & signs contract.

Customer makes payment.

Customer schedules delivery.
Postconditions: Vehicle is reserved or sold, depending on the action.

Use Case 05 – Upload Documents

Actors: Customer
Brief Description: Customer uploads required documents for verification (ID, proof of income, insurance). System validates them using OCR and simulated APIs.
Trigger: Customer proceeds to document upload step.
Preconditions: Customer must be logged in and have an account.
Flow of Activities:

Customer uploads ID/Driver’s License.

Customer uploads proof of income.

Customer uploads insurance certificate.

System runs OCR on ID documents and validates them.

System verifies insurance via simulated API.

System displays document preview with error/discrepancy flags.

System tracks upload status.

System requests additional documents if required.
Postconditions: Documents are verified and approved for financing/purchase.

Use Case 06 – Review Terms & Finalize Deal

Actors: Customer, Dealer/Admin
Brief Description: Customer reviews final purchase terms and signs the contract electronically.
Trigger: Required documents have been provided and vehicle is reserved.
Preconditions: Vehicle is available and reserved for the customer.
Flow of Activities:

System generates draft purchase agreement.

Customer reviews terms.

Customer requests clarifications (if needed).

Dealer sends final e-contract.

Customer signs electronically.

Dealer signs and finalizes contract.
Postconditions: A legally binding contract is created.

Use Case 07 – Complete Payment

Actors: Customer
Brief Description: Customer selects payment method (cash, financing, leasing) and completes transaction securely.
Trigger: Contract is finalized.
Preconditions: Payment terms are agreed upon.
Flow of Activities:

Customer selects payment method.

For cash: system provides secure payment instructions.

For financing: system confirms loan details and processes deposit.

For lease: system calculates monthly payments and integrates with provider.

System processes payment via secure banking APIs/Stripe.

System sends payment confirmation email.
Postconditions: Payment is recorded, and vehicle status updates to "Paid".

Use Case 08 – Schedule Delivery

Actors: Customer, Logistics Driver
Brief Description: Customer schedules delivery (dealer pickup or home delivery). Driver manages logistics.
Trigger: Purchase is confirmed and delivery method selected.
Preconditions: Vehicle is ready for delivery.
Flow of Activities:

Customer selects delivery option.

System confirms delivery choice.

Driver receives delivery assignment.

Driver picks up vehicle.

GPS tracking enabled for customer.

Driver marks “In Transit.”

System sends live updates.

Driver delivers vehicle and hands over digital key.

Driver marks delivery complete.

System sends delivery confirmation.
Postconditions: Vehicle successfully delivered.

Use Case 09 – Post-Purchase Support

Actors: Customer, Dealer Support Agent, System
Brief Description: Customer receives post-purchase services like warranty activation, manual, feedback, and service reminders.
Trigger: Vehicle delivery confirmed.
Preconditions: Vehicle has been delivered and customer has web access.
Flow of Activities:

System prompts warranty activation.

System sends digital car manual.

Customer provides feedback via survey.

System sends first service reminders.

Customer contacts dealer support if needed.
Postconditions: Customer is onboarded into after-sales support system.