tool
class_name ColoumnFill
extends Container

export var separation := 10 setget _set_separation


func _notification(what: int) -> void:
	if what == NOTIFICATION_SORT_CHILDREN:
		var x := -separation
		var y := 0
		var max_height := 0
		var height := 0
		for child in get_children():
			var min_size = max_vector(child.get_minimum_size(), child.rect_min_size)
			if x + separation + min_size.x > rect_size.x:
				height += max_height + separation
				y += max_height + separation
				max_height = 0
				x = -separation
			x += separation
			fit_child_in_rect(child, Rect2(Vector2(x, y), min_size))
			x += min_size.x
			max_height = max(max_height, min_size.y)
		height += max_height
		rect_min_size.y = height
		


func _set_separation(val: int) -> void:
	separation = val
	queue_sort()


static func max_vector(a: Vector2, b: Vector2) -> Vector2:
	return Vector2(max(a.x, b.x), max(a.y, b.y))

