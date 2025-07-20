// This script will fix the update ingredient functionality
// Add this script tag at the end of your index.html file, just before the closing </script> tag

// Override the update ingredient button click handler
document.addEventListener('DOMContentLoaded', function() {
    // Wait for the DOM to be fully loaded
    setTimeout(function() {
        // Get the update ingredient button
        const updateBtn = document.getElementById('updateIngredientBtn');
        
        // Remove existing event listeners by cloning the button
        const newUpdateBtn = updateBtn.cloneNode(true);
        updateBtn.parentNode.replaceChild(newUpdateBtn, updateBtn);
        
        // Add the new event listener with proper error handling
        newUpdateBtn.addEventListener('click', function() {
            const id = document.getElementById('editIngredientId').value;
            const name = document.getElementById('editIngredientName').value.trim();
            const quantity = parseInt(document.getElementById('editIngredientQuantity').value);
            
            if (!id || !name || isNaN(quantity) || quantity < 1) {
                alert('Please enter a valid ingredient name and quantity.');
                return;
            }
            
            // Create ingredient object
            const updatedIngredient = {
                id: id,
                name: name,
                quantity: quantity
            };
            
            console.log('Updating ingredient with data:', updatedIngredient);
            
            // Send PUT request to API
            fetch(`/api/ingredients/${id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(updatedIngredient)
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to update ingredient');
                }
                
                // Handle the response properly regardless of content
                if (response.status === 204) {
                    return {}; // No content response
                }
                
                return response.text().then(text => {
                    if (!text) return {};
                    try {
                        return JSON.parse(text);
                    } catch (e) {
                        console.log('Response was not valid JSON, continuing anyway');
                        return {};
                    }
                });
            })
            .then(data => {
                console.log('Update successful, response:', data);
                
                // Close modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('editIngredientModal'));
                modal.hide();
                
                // Reload ingredients
                loadIngredients();
            })
            .catch(error => {
                console.error('Error updating ingredient:', error);
                alert('Failed to update ingredient. Please try again.');
            });
        });
        
        console.log('Update ingredient button handler fixed!');
    }, 1000); // Wait 1 second to ensure all other scripts have loaded
});
