// Update Ingredient form
document.getElementById('updateIngredientBtn').addEventListener('click', function() {
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
